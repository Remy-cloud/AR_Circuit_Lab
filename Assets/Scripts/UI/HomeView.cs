using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARCircuitsLab.UI
{
    /// <summary>Home screen: start the experiments, open Free build, or see history.</summary>
    public class HomeView : ViewBase
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button freeBuildButton;
        [SerializeField] private Button historyButton;
        [SerializeField] private TMP_Text bestScoreText;

        private void Awake()
        {
            Bind(startButton, app.ShowBriefing);
            Bind(freeBuildButton, app.StartFreeBuild);
            Bind(historyButton, app.ShowHistory);
        }

        private void OnEnable()
        {
            int best = app.Data.BestScore;
            bestScoreText.text = best > 0 ? $"{best} points" : "No labs yet";
        }
    }
}
