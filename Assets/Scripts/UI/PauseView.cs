using UnityEngine;
using UnityEngine.UI;

namespace ARCircuitsLab.UI
{
    /// <summary>Pause menu shown over the frozen lab.</summary>
    public class PauseView : ViewBase
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button moveBenchButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        private void Awake()
        {
            Bind(resumeButton, app.Resume);
            Bind(moveBenchButton, app.MoveBench);
            Bind(restartButton, app.RestartLab);
            Bind(menuButton, app.GoToMenu);
        }
    }
}
