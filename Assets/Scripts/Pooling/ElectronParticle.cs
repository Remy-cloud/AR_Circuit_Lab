using UnityEngine;

namespace ARCircuitsLab.Pooling
{
    /// <summary>
    /// A small glowing dot that travels along a wire to show current flowing.
    /// It is pooled: when the circuit changes it is returned, not destroyed.
    /// </summary>
    public class ElectronParticle : MonoBehaviour, IPoolable
    {
        private Vector3[] path;
        private float length;
        private float speed;
        private float distance;
        private bool reverse;

        /// <summary>Follow a path (in the parent's local space) at a speed in metres per second.</summary>
        public void Travel(Vector3[] points, float pathLength, float metresPerSecond, float startOffset, bool backwards)
        {
            path = points;
            length = pathLength;
            speed = metresPerSecond;
            reverse = backwards;
            distance = Mathf.Repeat(startOffset, 1f) * length;
            Move();
        }

        private void Update()
        {
            if (path == null || length < 0.0001f) return;
            distance = Mathf.Repeat(distance + speed * Time.deltaTime, length);
            Move();
        }

        private void Move()
        {
            if (path == null || path.Length < 2) return;
            float d = reverse ? length - distance : distance;
            for (int k = 1; k < path.Length; k++)
            {
                float seg = Vector3.Distance(path[k - 1], path[k]);
                if (d <= seg || k == path.Length - 1)
                {
                    transform.localPosition = Vector3.Lerp(path[k - 1], path[k], seg > 0f ? Mathf.Clamp01(d / seg) : 0f);
                    return;
                }
                d -= seg;
            }
        }

        public void OnSpawn() => distance = 0f;

        public void OnDespawn()
        {
            path = null;
            speed = 0f;
        }
    }
}
