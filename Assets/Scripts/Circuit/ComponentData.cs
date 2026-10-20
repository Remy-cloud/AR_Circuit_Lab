using UnityEngine;

namespace ARCircuitsLab.Circuit
{
    /// <summary>
    /// Fixed values for one kind of part (configuration data).
    /// Changing a number here changes the part everywhere, without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "AR Circuits Lab/Component Data", fileName = "ComponentData")]
    public class ComponentData : ScriptableObject
    {
        public PartType partType;
        public string displayName = "Part";

        [Tooltip("Battery: voltage in volts. Other parts: resistance in ohms.")]
        public float value = 1f;

        [Tooltip("Battery only: its own small resistance (ohms).")]
        public float internalResistance = 0.2f;

        [Tooltip("Bulb only: power (watts) at which it shines at full brightness.")]
        public float ratedPower = 0.75f;
    }
}
