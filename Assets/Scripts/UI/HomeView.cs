using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARCircuitsLab.UI
{
    /// <summary>Home screen: start any of the five experiments, open Free build, or see history.</summary>
    public class HomeView : ViewBase
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button freeBuildButton;
        [SerializeField] private Button historyButton;
        [SerializeField] private Button[] experimentButtons;
        [SerializeField] private TMP_Text bestScoreText;

        private void Awake()
        {
            Bind(startButton, () => app.ShowBriefing(0));
            Bind(freeBuildButton, app.StartFreeBuild);
            Bind(historyButton, app.ShowHistory);
            for (int i = 0; i < experimentButtons.Length; i++)
            {
                int task = i;   // each row opens its own experiment
                Bind(experimentButtons[i], () => app.ShowBriefing(task));
            }
        }

        private void OnEnable()
        {
            int best = app.Data.BestScore;
            bestScoreText.text = best > 0 ? $"{best} points" : "No labs yet";
        }
    }
}
