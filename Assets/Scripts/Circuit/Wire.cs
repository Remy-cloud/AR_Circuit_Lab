using UnityEngine;

namespace ARCircuitsLab.Circuit
{
    /// <summary>
    /// A wire between two terminals, drawn as a small arc with a LineRenderer.
    /// The path is stored in the parent's local space so the wire moves with the bench.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class Wire : MonoBehaviour
    {
        /// <summary>A real wire has a tiny resistance; this also lets the solver find the current in it.</summary>
        public const float Resistance = 0.005f;

        [SerializeField] private int segments = 14;
        [SerializeField] private float minArc = 0.015f;
        [SerializeField] private float maxArc = 0.05f;
        [Tooltip("Thickness of the invisible tap area around the wire.")]
        [SerializeField] private float tapThickness = 0.022f;

        private LineRenderer line;
        private Vector3[] path;

        public Terminal A { get; private set; }
        public Terminal B { get; private set; }
        /// <summary>Current in amps (positive = from A to B).</summary>
        public float Current { get; private set; }
        public Vector3[] Path => path;
        public float Length { get; private set; }

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.useWorldSpace = false;
        }

        public void Connect(Terminal a, Terminal b)
        {
            A = a;
            B = b;
            Refresh();
        }

        public bool Joins(Terminal t) => A == t || B == t;
        public bool Connects(Terminal a, Terminal b) => (A == a && B == b) || (A == b && B == a);
        public void ApplyResult(float current) => Current = current;

        /// <summary>Rebuilds the arc between the two terminals.</summary>
        public void Refresh()
        {
            Vector3 start = transform.InverseTransformPoint(A.transform.position);
            Vector3 end = transform.InverseTransformPoint(B.transform.position);
            float arc = Mathf.Clamp(Vector3.Distance(start, end) * 0.25f, minArc, maxArc);

            path = new Vector3[segments + 1];
            Length = 0f;
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                path[i] = Vector3.Lerp(start, end, t) + Vector3.up * (arc * 4f * t * (1f - t));
                if (i > 0) Length += Vector3.Distance(path[i - 1], path[i]);
            }
            line.positionCount = path.Length;
            line.SetPositions(path);
            BuildTapArea();
        }

        // Invisible boxes along the wire so the student can tap it.
        // The two end pieces are left out, so taps near a terminal still reach the terminal.
        private void BuildTapArea()
        {
            for (int k = transform.childCount - 1; k >= 0; k--) Destroy(transform.GetChild(k).gameObject);
            for (int i = 2; i < path.Length - 2; i++)
            {
                Vector3 a = path[i - 1], b = path[i];
                var piece = new GameObject("Tap Area");
                piece.transform.SetParent(transform, false);
                piece.transform.localPosition = (a + b) * 0.5f;
                piece.transform.localRotation = Quaternion.LookRotation(b - a);
                var box = piece.AddComponent<BoxCollider>();
                box.size = new Vector3(tapThickness, tapThickness, Vector3.Distance(a, b) + 0.004f);
            }
        }
    }
}
