using TMPro;
using UnityEngine;
using ARCircuitsLab.Tasks;
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

        private void OnEnable()
        {
            var label = restartButton.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = app.Mode == LabMode.FreeBuild ? "Clear the bench" : "Restart lab";
        }
    }
}
