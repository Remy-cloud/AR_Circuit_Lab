using UnityEngine;

namespace ARCircuitsLab.UI
{
    /// <summary>Keeps the UI inside the iPhone's safe area (away from the notch and home bar).</summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        private Rect applied;

        private void Update()
        {
            if (Screen.safeArea != applied) Apply();
        }

        private void Apply()
        {
            applied = Screen.safeArea;
            var rt = (RectTransform)transform;
            Vector2 min = applied.position, max = applied.position + applied.size;
            min.x /= Screen.width; min.y /= Screen.height;
            max.x /= Screen.width; max.y /= Screen.height;
            rt.anchorMin = min; rt.anchorMax = max;
        }
    }
}
