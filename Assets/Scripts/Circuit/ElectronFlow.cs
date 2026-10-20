using System.Collections.Generic;
using UnityEngine;
using ARCircuitsLab.Pooling;

namespace ARCircuitsLab.Circuit
{
    /// <summary>
    /// Shows current as electrons moving along the wires.
    /// Electrons come from an object pool, so nothing is created or destroyed while the student works.
    /// Electrons move from - to +, the opposite way to conventional current.
    /// </summary>
    public class ElectronFlow : MonoBehaviour
    {
        [SerializeField] private int poolSize = 100;
        [SerializeField] private float spacing = 0.03f;
        [SerializeField] private float minCurrent = 0.01f;
        [SerializeField] private float baseSpeed = 0.03f;
        [SerializeField] private float speedPerAmp = 0.3f;
        [SerializeField] private float maxSpeed = 0.4f;

        private ObjectPool<ElectronParticle> pool;

        public int ActiveElectrons => pool?.ActiveCount ?? 0;

        public void Init(ElectronParticle prefab, Transform parent)
        {
            pool = new ObjectPool<ElectronParticle>(prefab, poolSize, parent);
        }

        /// <summary>The pooled objects were destroyed with the bench.</summary>
        public void Clear() => pool = null;

        public void Show(IReadOnlyList<Wire> wires)
        {
            if (pool == null) return;
            pool.ReleaseAll();

            foreach (Wire w in wires)
            {
                float amps = Mathf.Abs(w.Current);
                if (amps < minCurrent || w.Path == null) continue;

                int count = Mathf.Max(1, Mathf.RoundToInt(w.Length / spacing));
                float speed = Mathf.Min(maxSpeed, baseSpeed + speedPerAmp * amps);
                bool backwards = w.Current > 0f;   // conventional current A → B, so electrons go B → A
                for (int k = 0; k < count; k++)
                    pool.Get().Travel(w.Path, w.Length, speed, k / (float)count, backwards);
            }
        }
    }
}
