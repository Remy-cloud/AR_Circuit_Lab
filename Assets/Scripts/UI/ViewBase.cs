using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ARCircuitsLab.Events;
using ARCircuitsLab.States;

namespace ARCircuitsLab.UI
{
    /// <summary>
    /// Base class for every screen. Gives access to the app flow and wires
    /// buttons so each click also plays the click sound (via an event).
    /// </summary>
    public abstract class ViewBase : MonoBehaviour
    {
        [SerializeField] protected AppStateManager app;

        protected void Bind(Button button, UnityAction action)
        {
            if (button == null) return;
            button.onClick.AddListener(() =>
            {
                GameEvents.RaiseButtonClicked();
                action();
            });
        }

        protected static string FormatTime(float seconds)
        {
            int s = Mathf.Max(0, Mathf.RoundToInt(seconds));
            return $"{s / 60}:{s % 60:00}";
        }
    }
}
