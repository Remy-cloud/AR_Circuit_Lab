using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using ARCircuitsLab.Events;
using ARCircuitsLab.States;

namespace ARCircuitsLab.Circuit
{
    /// <summary>
    /// Turns screen touches into actions on the bench:
    /// tap = place / wire / toggle / remove a wire, press and hold = remove a part (or a terminal's wires).
    /// Only active during the simulation, and ignores touches on the UI.
    /// </summary>
    public class CircuitInput : MonoBehaviour
    {
        [SerializeField] private CircuitManager circuit;
        [SerializeField] private float holdSeconds = 0.5f;
        [SerializeField] private float maxTapMove = 40f;     // pixels
        [SerializeField] private float maxDistance = 5f;     // metres

        private static readonly List<RaycastResult> UiHits = new List<RaycastResult>();

        private Camera cam;
        private bool active;
        private bool pressing;
        private bool holdDone;
        private float pressTime;
        private Vector2 pressPosition;
        private Collider pressTarget;

        private void Awake() => cam = Camera.main;
        private void OnEnable() => GameEvents.AppStateChanged += OnState;
        private void OnDisable() => GameEvents.AppStateChanged -= OnState;

        private void OnState(AppStateId state)
        {
            active = state == AppStateId.Simulation;
            pressing = false;
        }

        private void Update()
        {
            Pointer pointer = Pointer.current;
            if (!active || pointer == null || !circuit.IsReady) return;
            Vector2 position = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                pressing = !IsOverUI(position);
                if (!pressing) return;
                holdDone = false;
                pressTime = Time.unscaledTime;
                pressPosition = position;
                pressTarget = RaycastAt(position);
            }
            if (!pressing) return;

            // Press and hold on a part removes it.
            if (!holdDone && pointer.press.isPressed && Time.unscaledTime - pressTime >= holdSeconds)
            {
                holdDone = true;
                if ((position - pressPosition).magnitude <= maxTapMove && pressTarget != null)
                {
                    if (pressTarget.TryGetComponent(out Terminal terminal)) circuit.HoldTerminal(terminal);
                    else if (pressTarget.GetComponentInParent<Wire>() is Wire held) circuit.TapWire(held);
                    else if (pressTarget.GetComponentInParent<CircuitComponent>() is CircuitComponent part) circuit.HoldPart(part);
                }
            }

            if (pointer.press.wasReleasedThisFrame)
            {
                pressing = false;
                if (holdDone || (position - pressPosition).magnitude > maxTapMove) return;
                HandleTap(pressTarget);
            }
        }

        private void HandleTap(Collider hit)
        {
            if (hit == null) { circuit.TapNothing(); return; }

            if (hit.TryGetComponent(out Terminal terminal)) { circuit.TapTerminal(terminal); return; }
            Wire wire = hit.GetComponentInParent<Wire>();
            if (wire != null) { circuit.TapWire(wire); return; }
            CircuitComponent part = hit.GetComponentInParent<CircuitComponent>();
            if (part != null) { circuit.TapPart(part); return; }
            BenchSlot slot = hit.GetComponentInParent<BenchSlot>();
            if (slot != null) { circuit.TapSlot(slot); return; }
            circuit.TapNothing();
        }

        private Collider RaycastAt(Vector2 screen)
        {
            if (cam == null) cam = Camera.main;
            Ray ray = cam.ScreenPointToRay(screen);
            return Physics.Raycast(ray, out RaycastHit hit, maxDistance) ? hit.collider : null;
        }

        private static bool IsOverUI(Vector2 screen)
        {
            if (EventSystem.current == null) return false;
            var data = new PointerEventData(EventSystem.current) { position = screen };
            UiHits.Clear();
            EventSystem.current.RaycastAll(data, UiHits);
            return UiHits.Count > 0;
        }
    }
}
