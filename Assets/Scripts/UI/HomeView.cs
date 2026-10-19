using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARCircuitsLab.UI
{
    /// <summary>Home screen: start the lab or open history.</summary>
    public class HomeView : ViewBase
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button historyButton;
        [SerializeField] private TMP_Text bestScoreText;

        private void Awake()
        {
            Bind(startButton, app.ShowBriefing);
            Bind(historyButton, app.ShowHistory);
        }

        private void OnEnable()
        {
            int best = app.Data.BestScore;
            bestScoreText.text = best > 0 ? $"{best} points" : "No labs yet";
        }
    }
}
