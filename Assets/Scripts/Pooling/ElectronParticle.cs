using UnityEngine;

namespace ARCircuitsLab.Pooling
{
    /// <summary>
    /// A small glowing dot that travels along a wire to show current flowing.
    /// It is pooled: when the circuit changes it is returned, not destroyed.
    /// </summary>
    public class ElectronParticle : MonoBehaviour, IPoolable
    {
        private Vector3 start;
        private Vector3 end;
        private float speed;
        private float progress;

        public void Travel(Vector3 from, Vector3 to, float unitsPerSecond, float startOffset)
        {
            start = from;
            end = to;
            speed = unitsPerSecond;
            progress = Mathf.Repeat(startOffset, 1f);
            transform.position = Vector3.Lerp(start, end, progress);
        }

        private void Update()
        {
            float length = Vector3.Distance(start, end);
            if (length < 0.0001f) return;
            progress = Mathf.Repeat(progress + speed * Time.deltaTime / length, 1f);
            transform.position = Vector3.Lerp(start, end, progress);
        }

        public void OnSpawn() => progress = 0f;

        public void OnDespawn()
        {
            speed = 0f;
            transform.position = Vector3.zero;
        }
    }
}
