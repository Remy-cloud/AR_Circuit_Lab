using UnityEngine;

namespace ARCircuitsLab.Circuit
{
    /// <summary>
    /// Base class for every part on the bench. Each part has two terminals (A and B)
    /// and tells the solver its resistance; the solver then hands back the
    /// voltage and current, which each part shows in its own way.
    /// </summary>
    public abstract class CircuitComponent : MonoBehaviour
    {
        /// <summary>Used for parts that block current (open switch, broken bulb).</summary>
        public const float Open = float.PositiveInfinity;

        [SerializeField] protected ComponentData data;
        [SerializeField] private Terminal terminalA;
        [SerializeField] private Terminal terminalB;
        [SerializeField] private bool isWorking = true;

        public ComponentData Data => data;
        public PartType Type => data.partType;
        public Terminal A => terminalA;
        public Terminal B => terminalB;
        public bool IsWorking => isWorking;
        public BenchSlot Slot { get; set; }

        /// <summary>Current through the part in amps (positive = from A to B).</summary>
        public float Current { get; private set; }
        /// <summary>Voltage across the part in volts.</summary>
        public float Voltage { get; private set; }

        protected virtual void Awake()
        {
            terminalA.Init(this, 0);
            terminalB.Init(this, 1);
        }

        /// <summary>Resistance between A and B in ohms, or <see cref="Open"/>.</summary>
        public abstract float GetResistance();

        public virtual void SetWorking(bool working) => isWorking = working;

        /// <summary>Called by the Circuit Manager after every recalculation.</summary>
        public virtual void ApplyResult(float voltage, float current)
        {
            Voltage = voltage;
            Current = current;
        }

        public Terminal GetTerminal(int index) => index == 0 ? terminalA : terminalB;
    }
}
