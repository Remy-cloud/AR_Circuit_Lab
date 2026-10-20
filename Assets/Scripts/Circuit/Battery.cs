namespace ARCircuitsLab.Circuit
{
    /// <summary>Power source. Terminal A is negative (-), terminal B is positive (+).</summary>
    public class Battery : CircuitComponent
    {
        public float Emf => data.value;

        // A battery has a small resistance of its own; it limits the current in a short circuit.
        public override float GetResistance() => data.internalResistance;
    }
}
