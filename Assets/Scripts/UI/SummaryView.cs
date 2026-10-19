using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ARCircuitsLab.Data;

namespace ARCircuitsLab.UI
{
    /// <summary>Completion screen: outcome, performance metrics, restart and main menu.</summary>
    public class SummaryView : ViewBase
    {
        [SerializeField] private TMP_Text headlineText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text experimentsText;
        [SerializeField] private TMP_Text mistakesText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private int experimentCount = 3;

        private void Awake()
        {
            Bind(restartButton, app.RestartLab);
            Bind(menuButton, app.GoToMenu);
        }

        private void OnEnable()
        {
            SessionRecord r = app.Data.LastSession;
            if (r == null) return;
            bool all = r.tasksCompleted >= experimentCount;
            headlineText.text = all ? "Lab complete!" : "Lab ended";
            scoreText.text = r.score.ToString();
            timeText.text = FormatTime(r.timeTaken);
            experimentsText.text = $"{r.tasksCompleted} / {experimentCount}";
            mistakesText.text = r.mistakes.ToString();
        }
    }
}
