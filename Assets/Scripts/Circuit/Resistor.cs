namespace ARCircuitsLab.Circuit
{
    /// <summary>Fixed resistor: reduces the current in its branch.</summary>
    public class Resistor : CircuitComponent
    {
        public override float GetResistance() => IsWorking ? data.value : Open;
    }
}
