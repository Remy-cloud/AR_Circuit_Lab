using UnityEngine;

namespace ARCircuitsLab.Circuit
{
    /// <summary>Opens or closes the path. An open switch stops all current (FR-05).</summary>
    public class Switch : CircuitComponent
    {
        [SerializeField] private Transform lever;
        [SerializeField] private float openAngle = 35f;
        [SerializeField] private bool isOn;

        public bool IsOn => isOn;

        protected override void Awake()
        {
            base.Awake();
            UpdateLever();
        }

        public void Toggle() => SetOn(!isOn);

        public void SetOn(bool on)
        {
            isOn = on;
            UpdateLever();
        }

        public override float GetResistance() => isOn && IsWorking ? data.value : Open;

        private void UpdateLever()
        {
            if (lever != null) lever.localRotation = Quaternion.Euler(0f, 0f, isOn ? 0f : openAngle);
        }
    }
}
