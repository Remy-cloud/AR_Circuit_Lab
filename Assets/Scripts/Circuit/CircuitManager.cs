using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARCircuitsLab.AssetLoading;
using ARCircuitsLab.Events;
using ARCircuitsLab.Pooling;

namespace ARCircuitsLab.Circuit
{
    /// <summary>
    /// Circuit Manager: holds every part and wire on the bench, handles placing,
    /// wiring, removing and undo, recalculates the circuit after each change
    /// and reports short circuits. It has no UI code; it only raises events.
    /// </summary>
    public class CircuitManager : MonoBehaviour
    {
        private const string SlotAsset = "Bench Slot";
        private const string WireAsset = "Wire";
        private const string ElectronAsset = "Electron";

        [Header("References")]
        [SerializeField] private AssetManager assets;
        [SerializeField] private ElectronFlow electronFlow;

        [Header("Bench layout")]
        [SerializeField] private int columns = 4;
        [SerializeField] private int rows = 2;
        [SerializeField] private Vector2 spacing = new Vector2(0.14f, 0.16f);
        [SerializeField] private float benchTop = 0.031f;

        [Header("Safety")]
        [Tooltip("Battery current (A) above which we call it a short circuit.")]
        [SerializeField] private float shortCircuitCurrent = 1.5f;

        private readonly List<BenchSlot> slots = new List<BenchSlot>();
        private readonly List<CircuitComponent> parts = new List<CircuitComponent>();
        private readonly List<Wire> wires = new List<Wire>();
        private readonly Stack<UndoStep> history = new Stack<UndoStep>();
        private readonly CircuitSolver solver = new CircuitSolver();

        private Transform bench;
        private Transform wiresRoot;
        private PartType? selectedPart;
        private Terminal selectedTerminal;
        private bool wasShort;
        private Coroutine messageRoutine;

        public bool IsReady => bench != null;
        public IReadOnlyList<CircuitComponent> Parts => parts;
        public IReadOnlyList<Wire> Wires => wires;
        public IReadOnlyList<BenchSlot> Slots => slots;
        public float BatteryCurrent { get; private set; }
        public bool IsShortCircuit { get; private set; }
        public bool IsClosed => BatteryCurrent > 0.01f;

        private void OnEnable()
        {
            GameEvents.PartSelected += SelectPart;
            GameEvents.UndoRequested += Undo;
        }

        private void OnDisable()
        {
            GameEvents.PartSelected -= SelectPart;
            GameEvents.UndoRequested -= Undo;
        }

        // ------------------------------------------------------------ bench

        /// <summary>Lays out the slots on a newly placed bench.</summary>
        public void AttachToBench(Transform newBench)
        {
            if (bench == newBench) return;   // same bench, just moved
            Detach();
            bench = newBench;

            GameObject slotPrefab = assets.GetPrefab(SlotAsset);
            var slotRoot = new GameObject("Slots").transform;
            slotRoot.SetParent(bench, false);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    // Slot 0 is top-left as the student looks at the bench.
                    float x = ((columns - 1) / 2f - c) * spacing.x;
                    float z = (r - (rows - 1) / 2f) * spacing.y;
                    BenchSlot slot = Instantiate(slotPrefab, slotRoot).GetComponent<BenchSlot>();
                    slot.transform.localPosition = new Vector3(x, benchTop, z);
                    slot.transform.localRotation = Quaternion.identity;
                    slot.Index = slots.Count;
                    slot.name = "Slot " + slot.Index;
                    slots.Add(slot);
                }
            }

            wiresRoot = new GameObject("Wires").transform;
            wiresRoot.SetParent(bench, false);

            GameObject electron = assets.GetPrefab(ElectronAsset);
            if (electron != null) electronFlow.Init(electron.GetComponent<ElectronParticle>(), wiresRoot);
            Recalculate();
        }

        /// <summary>Forget the bench (it is destroyed by the AR manager).</summary>
        public void Detach()
        {
            ClearSelection();
            slots.Clear();
            parts.Clear();
            wires.Clear();
            history.Clear();
            electronFlow.Clear();
            bench = null;
            wasShort = IsShortCircuit = false;
            BatteryCurrent = 0f;
        }

        /// <summary>Removes every part and wire but keeps the bench.</summary>
        public void ClearCircuit()
        {
            ClearSelection();
            foreach (Wire w in wires) Destroy(w.gameObject);
            foreach (CircuitComponent p in parts)
            {
                p.Slot.Occupant = null;
                Destroy(p.gameObject);
            }
            wires.Clear();
            parts.Clear();
            history.Clear();
            wasShort = false;
            Recalculate();
        }

        /// <summary>Nothing done before this point can be undone (used after a ready-made circuit).</summary>
        public void ClearUndoHistory() => history.Clear();

        // ------------------------------------------------------------ student input

        public void SelectPart(PartType type)
        {
            if (!IsReady) return;
            if (selectedPart == type) { ClearSelection(); return; }

            ClearSelection();
            selectedPart = type;
            bool anyFree = false;
            foreach (BenchSlot s in slots)
            {
                s.SetHighlight(s.IsEmpty);
                anyFree |= s.IsEmpty;
            }
            GameEvents.RaiseSelectedPartChanged(type);
            Instruction(anyFree
                ? $"{DescriptionOf(type)} Tap a glowing spot to place it."
                : "The bench is full. Press and hold a part to remove it.");
        }

        public void TapSlot(BenchSlot slot)
        {
            if (selectedPart == null || !slot.IsEmpty) { ClearSelection(); return; }
            PartType type = selectedPart.Value;
            ClearSelection();
            Place(type, slot.Index);
        }

        public void TapPart(CircuitComponent part)
        {
            if (selectedTerminal != null || selectedPart != null) { ClearSelection(); return; }

            if (part is Switch sw)
            {
                sw.Toggle();
                GameEvents.RaiseSwitchToggled(sw.IsOn);
                Recalculate();
                Message(sw.IsOn ? "Switch closed." : "Switch open.");
                return;
            }
            Message(Describe(part));
        }

        public void HoldPart(CircuitComponent part)
        {
            ClearSelection();
            Remove(part);
            Message($"{part.Data.displayName} removed. Tap Undo to bring it back.");
        }

        /// <summary>Press and hold a terminal: removes the wires connected to it.</summary>
        public void HoldTerminal(Terminal t)
        {
            ClearSelection();
            var removed = new List<(TerminalRef, TerminalRef)>();
            for (int k = wires.Count - 1; k >= 0; k--)
            {
                Wire w = wires[k];
                if (!w.Joins(t)) continue;
                removed.Add((Ref(w.A), Ref(w.B)));
                wires.RemoveAt(k);
                Destroy(w.gameObject);
            }
            if (removed.Count == 0) { Message("There are no wires on this terminal."); return; }

            history.Push(UndoStep.WiresRemoved(removed));
            Recalculate();
            Message(removed.Count == 1 ? "Wire removed. Tap Undo to bring it back."
                                       : $"{removed.Count} wires removed. Tap Undo to bring them back.");
        }

        public void TapTerminal(Terminal t)
        {
            if (selectedPart != null) ClearSelection();

            if (selectedTerminal == null)
            {
                selectedTerminal = t;
                t.SetHighlight(true);
                Instruction("Now tap another terminal to join them with a wire.");
                return;
            }
            if (selectedTerminal == t) { ClearSelection(); return; }

            Terminal first = selectedTerminal;
            ClearSelection();
            if (FindWire(first, t) != null) { Message("Those two are already joined."); return; }
            Connect(first, t);
        }

        public void TapNothing() => ClearSelection();

        // ------------------------------------------------------------ building

        public CircuitComponent Place(PartType type, int slotIndex, bool working = true, bool record = true)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count || !slots[slotIndex].IsEmpty) return null;
            GameObject prefab = assets.GetPrefab(type.ToString());
            if (prefab == null) { Debug.LogWarning($"[CircuitManager] No prefab for {type}"); return null; }

            BenchSlot slot = slots[slotIndex];
            CircuitComponent part = Instantiate(prefab, slot.transform).GetComponent<CircuitComponent>();
            part.transform.localPosition = Vector3.zero;
            part.transform.localRotation = Quaternion.identity;
            part.Slot = slot;
            part.SetWorking(working);
            slot.Occupant = part;
            parts.Add(part);

            if (record) history.Push(UndoStep.Placed(slotIndex));
            GameEvents.RaiseComponentPlaced();
            Recalculate();
            return part;
        }

        public Wire Connect(Terminal a, Terminal b, bool record = true)
        {
            Wire wire = Instantiate(assets.GetPrefab(WireAsset), wiresRoot).GetComponent<Wire>();
            wire.transform.localPosition = Vector3.zero;
            wire.transform.localRotation = Quaternion.identity;
            wire.Connect(a, b);
            wires.Add(wire);

            if (record) history.Push(UndoStep.Wired(Ref(a), Ref(b)));
            GameEvents.RaiseWireConnected();
            Recalculate();
            return wire;
        }

        public void Remove(CircuitComponent part, bool record = true)
        {
            var attached = new List<(TerminalRef, TerminalRef)>();
            for (int k = wires.Count - 1; k >= 0; k--)
            {
                Wire w = wires[k];
                if (w.A.Owner != part && w.B.Owner != part) continue;
                attached.Add((Ref(w.A), Ref(w.B)));
                wires.RemoveAt(k);
                Destroy(w.gameObject);
            }

            if (record)
                history.Push(UndoStep.Removed(part.Type, part.Slot.Index, part.IsWorking,
                    part is Switch sw && sw.IsOn, attached));

            parts.Remove(part);
            part.Slot.Occupant = null;
            Destroy(part.gameObject);
            Recalculate();
        }

        public void Undo()
        {
            ClearSelection();
            if (history.Count == 0) { Message("Nothing to undo."); return; }

            UndoStep step = history.Pop();
            switch (step.kind)
            {
                case UndoStep.Kind.Placed:
                    CircuitComponent placed = slots[step.slot].Occupant;
                    if (placed != null) Remove(placed, false);
                    break;

                case UndoStep.Kind.Wired:
                    Wire w = FindWire(Resolve(step.a), Resolve(step.b));
                    if (w != null)
                    {
                        wires.Remove(w);
                        Destroy(w.gameObject);
                        Recalculate();
                    }
                    break;

                case UndoStep.Kind.WiresRemoved:
                    foreach ((TerminalRef a, TerminalRef b) in step.wires)
                    {
                        Terminal ta = Resolve(a), tb = Resolve(b);
                        if (ta != null && tb != null) Connect(ta, tb, false);
                    }
                    break;

                case UndoStep.Kind.Removed:
                    CircuitComponent back = Place(step.type, step.slot, step.working, false);
                    if (back is Switch s) s.SetOn(step.switchOn);
                    foreach ((TerminalRef a, TerminalRef b) in step.wires)
                    {
                        Terminal ta = Resolve(a), tb = Resolve(b);
                        if (ta != null && tb != null) Connect(ta, tb, false);
                    }
                    Recalculate();
                    break;
            }
        }

        // ------------------------------------------------------------ simulation

        /// <summary>Solves the circuit and tells everyone about it (FR-06, FR-07).</summary>
        public void Recalculate()
        {
            CircuitSolver.Result r = solver.Solve(parts, wires, shortCircuitCurrent);
            BatteryCurrent = r.batteryCurrent;
            IsShortCircuit = r.shortCircuit;
            electronFlow.Show(wires);

            GameEvents.RaiseCircuitUpdated();
            if (IsShortCircuit && !wasShort) GameEvents.RaiseShortCircuit();
            wasShort = IsShortCircuit;
        }

        public T Find<T>(System.Predicate<T> match = null) where T : CircuitComponent
        {
            foreach (CircuitComponent p in parts)
                if (p is T t && (match == null || match(t))) return t;
            return null;
        }

        public int Count<T>(System.Predicate<T> match = null) where T : CircuitComponent
        {
            int n = 0;
            foreach (CircuitComponent p in parts)
                if (p is T t && (match == null || match(t))) n++;
            return n;
        }

        // ------------------------------------------------------------ helpers

        private Wire FindWire(Terminal a, Terminal b)
        {
            if (a == null || b == null) return null;
            foreach (Wire w in wires) if (w.Connects(a, b)) return w;
            return null;
        }

        private void ClearSelection()
        {
            bool hadSelection = selectedPart != null || selectedTerminal != null;
            selectedPart = null;
            foreach (BenchSlot s in slots) if (s != null) s.SetHighlight(false);
            if (selectedTerminal != null) selectedTerminal.SetHighlight(false);
            selectedTerminal = null;
            if (hadSelection)
            {
                GameEvents.RaiseSelectedPartChanged(null);
                Instruction("");
            }
        }

        private void Instruction(string text)
        {
            if (messageRoutine != null) StopCoroutine(messageRoutine);
            messageRoutine = null;
            GameEvents.RaiseInstructionChanged(text);
        }

        // A short message that disappears by itself.
        private void Message(string text)
        {
            Instruction(text);
            messageRoutine = StartCoroutine(ClearLater(4f));
        }

        private IEnumerator ClearLater(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            messageRoutine = null;
            GameEvents.RaiseInstructionChanged("");
        }

        private static string Describe(CircuitComponent p)
        {
            if (!p.IsWorking) return $"This {p.Data.displayName.ToLower()} is broken. Press and hold it to remove it.";
            string reading = $"{Mathf.Abs(p.Current):0.00} A, {Mathf.Abs(p.Voltage):0.0} V";
            return p is Bulb b
                ? $"Bulb: {reading}, {Mathf.RoundToInt(b.Brightness * 100)}% bright"
                : $"{p.Data.displayName}: {reading}";
        }

        private string DescriptionOf(PartType t)
        {
            GameObject prefab = assets.GetPrefab(t.ToString());
            CircuitComponent part = prefab != null ? prefab.GetComponent<CircuitComponent>() : null;
            return part != null && !string.IsNullOrEmpty(part.Data.description)
                ? part.Data.description
                : $"{t}.";
        }

        private static TerminalRef Ref(Terminal t) => new TerminalRef(t.Owner.Slot.Index, t.Index);

        private Terminal Resolve(TerminalRef r)
        {
            if (r.slot < 0 || r.slot >= slots.Count) return null;
            CircuitComponent p = slots[r.slot].Occupant;
            return p != null ? p.GetTerminal(r.terminal) : null;
        }

        // Undo remembers positions (slot + terminal), not objects, because objects
        // can be destroyed and created again by later steps.
        private readonly struct TerminalRef
        {
            public readonly int slot, terminal;
            public TerminalRef(int slot, int terminal) { this.slot = slot; this.terminal = terminal; }
        }

        private class UndoStep
        {
            public enum Kind { Placed, Wired, Removed, WiresRemoved }
            public Kind kind;
            public int slot;
            public PartType type;
            public bool working, switchOn;
            public TerminalRef a, b;
            public List<(TerminalRef, TerminalRef)> wires;

            public static UndoStep Placed(int slot) => new UndoStep { kind = Kind.Placed, slot = slot };
            public static UndoStep Wired(TerminalRef a, TerminalRef b) => new UndoStep { kind = Kind.Wired, a = a, b = b };
            public static UndoStep WiresRemoved(List<(TerminalRef, TerminalRef)> wires) =>
                new UndoStep { kind = Kind.WiresRemoved, wires = wires };
            public static UndoStep Removed(PartType type, int slot, bool working, bool switchOn,
                List<(TerminalRef, TerminalRef)> wires) =>
                new UndoStep { kind = Kind.Removed, type = type, slot = slot, working = working, switchOn = switchOn, wires = wires };
        }
    }
}
