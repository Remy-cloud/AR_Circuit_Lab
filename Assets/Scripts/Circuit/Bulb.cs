using UnityEngine;

namespace ARCircuitsLab.Circuit
{
    /// <summary>Light bulb: glows in proportion to the power it receives (FR-06).</summary>
    public class Bulb : CircuitComponent
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Renderer glass;
        [Tooltip("Which material of the renderer is the glass (-1 = all of them).")]
        [SerializeField] private int glassMaterialIndex = -1;
        [SerializeField] private Light glow;
        [SerializeField] private Color offColor = new Color(0.92f, 0.92f, 0.88f);
        [SerializeField] private Color onColor = new Color(1f, 0.85f, 0.35f);
        [SerializeField] private Color brokenColor = new Color(0.35f, 0.33f, 0.32f);
        [SerializeField] private float maxLightIntensity = 1.2f;

        private MaterialPropertyBlock block;
        private float brightness;

        /// <summary>0 = off, 1 = full brightness.</summary>
        public float Brightness => brightness;
        public bool IsLit => brightness > 0.05f;

        protected override void Awake()
        {
            base.Awake();
            block = new MaterialPropertyBlock();
            Show();
        }

        public override float GetResistance() => IsWorking ? data.value : Open;

        public override void SetWorking(bool working)
        {
            base.SetWorking(working);
            Show();
        }

        public override void ApplyResult(float voltage, float current)
        {
            base.ApplyResult(voltage, current);
            float power = Mathf.Abs(voltage * current);
            brightness = IsWorking ? Mathf.Clamp01(power / data.ratedPower) : 0f;
            Show();
        }

        private void Show()
        {
            if (glass == null) return;
            Color body = !IsWorking ? brokenColor : Color.Lerp(offColor, onColor, brightness);
            if (glassMaterialIndex >= 0) glass.GetPropertyBlock(block, glassMaterialIndex);
            else glass.GetPropertyBlock(block);
            block.SetColor(BaseColorId, body);
            block.SetColor(EmissionId, onColor * (brightness * 2.5f));
            if (glassMaterialIndex >= 0) glass.SetPropertyBlock(block, glassMaterialIndex);
            else glass.SetPropertyBlock(block);

            if (glow != null)
            {
                glow.enabled = brightness > 0.01f;
                glow.intensity = brightness * maxLightIntensity;
            }
        }
    }
}
