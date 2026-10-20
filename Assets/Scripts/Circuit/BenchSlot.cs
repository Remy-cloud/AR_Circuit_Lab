using UnityEngine;

namespace ARCircuitsLab.Circuit
{
    /// <summary>A spot on the bench where one part can be placed.</summary>
    public class BenchSlot : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer visual;
        [SerializeField] private Color idleColor = new Color(0.72f, 0.55f, 0.37f);
        [SerializeField] private Color highlightColor = new Color(1f, 0.79f, 0.24f);

        private MaterialPropertyBlock block;

        public int Index { get; set; }
        public CircuitComponent Occupant { get; set; }
        public bool IsEmpty => Occupant == null;

        private void Awake() => SetHighlight(false);

        public void SetHighlight(bool on)
        {
            if (visual == null) return;
            block ??= new MaterialPropertyBlock();
            block.SetColor(BaseColorId, on ? highlightColor : idleColor);
            visual.SetPropertyBlock(block);
        }
    }
}
