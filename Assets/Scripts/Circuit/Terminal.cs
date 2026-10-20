using UnityEngine;

namespace ARCircuitsLab.Circuit
{
    /// <summary>One connection point on a part. Wires join two terminals.</summary>
    public class Terminal : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer visual;
        [SerializeField] private Color highlightColor = new Color(1f, 0.79f, 0.24f);

        private MaterialPropertyBlock block;
        private Vector3 normalScale;

        public CircuitComponent Owner { get; private set; }
        public int Index { get; private set; }

        public void Init(CircuitComponent owner, int index)
        {
            Owner = owner;
            Index = index;
            normalScale = transform.localScale;
        }

        public void SetHighlight(bool on)
        {
            if (visual == null) return;
            block ??= new MaterialPropertyBlock();
            if (on)
            {
                block.SetColor(BaseColorId, highlightColor);
                visual.SetPropertyBlock(block);
            }
            else
            {
                visual.SetPropertyBlock(null);
            }
            transform.localScale = on ? normalScale * 1.5f : normalScale;
        }
    }
}
