using UnityEngine;
using UnityEngine.UI;

namespace ARCircuitsLab.UI
{
    /// <summary>Explains the experiment before the camera opens.</summary>
    public class BriefingView : ViewBase
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Button findTableButton;

        private void Awake()
        {
            Bind(backButton, app.GoToMenu);
            Bind(findTableButton, app.StartLab);
        }
    }
}
