using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ARCircuitsLab.Circuit;
using ARCircuitsLab.Data;
using ARCircuitsLab.Events;

namespace ARCircuitsLab.UI
{
    /// <summary>
    /// In-lab HUD: current experiment, live readings, hints, the parts drawer,
    /// success feedback and the short-circuit warning.
    /// </summary>
    public class HudView : ViewBase
    {
        [Header("Goal card")]
        [SerializeField] private TMP_Text experimentText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text goalText;
        [SerializeField] private Button pauseButton;

        [Header("Live readings and hint")]
        [SerializeField] private TMP_Text currentText;
        [SerializeField] private TMP_Text circuitText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private GameObject hintBubble;
        [SerializeField] private TMP_Text hintText;

        [Header("Parts drawer")]
        [SerializeField] private Button batteryButton;
        [SerializeField] private Button switchButton;
        [SerializeField] private Button bulbButton;
        [SerializeField] private Button resistorButton;
        [SerializeField] private Button undoButton;

        [Header("Feedback")]
        [SerializeField] private GameObject successSheet;
        [SerializeField] private TMP_Text successTitle;
        [SerializeField] private TMP_Text successBody;
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject warningBanner;
        [SerializeField] private Button warningUndoButton;

        private void Awake()
        {
            Bind(pauseButton, app.Pause);
            Bind(batteryButton, () => GameEvents.RaisePartSelected(PartType.Battery));
            Bind(switchButton, () => GameEvents.RaisePartSelected(PartType.Switch));
            Bind(bulbButton, () => GameEvents.RaisePartSelected(PartType.Bulb));
            Bind(resistorButton, () => GameEvents.RaisePartSelected(PartType.Resistor));
            Bind(undoButton, GameEvents.RaiseUndoRequested);
            Bind(continueButton, () => successSheet.SetActive(false));
            Bind(warningUndoButton, () =>
            {
                warningBanner.SetActive(false);
                GameEvents.RaiseUndoRequested();
            });
        }

        private void OnEnable()
        {
            GameEvents.ProgressChanged += ShowProgress;
            GameEvents.FeedbackShown += ShowSuccess;
            GameEvents.ShortCircuit += ShowWarning;
            successSheet.SetActive(false);
            warningBanner.SetActive(false);
            ShowProgress(GameEvents.LastProgress);
        }

        private void OnDisable()
        {
            GameEvents.ProgressChanged -= ShowProgress;
            GameEvents.FeedbackShown -= ShowSuccess;
            GameEvents.ShortCircuit -= ShowWarning;
        }

        private void ShowProgress(SessionProgress p)
        {
            if (p.experimentCount == 0) return;   // nothing sent yet
            experimentText.text = $"Experiment {p.experimentNumber} of {p.experimentCount}";
            titleText.text = p.title;
            goalText.text = p.goal;
            currentText.text = $"{p.current:0.00} A";
            circuitText.text = p.circuitClosed ? "Closed" : "Open";
            scoreText.text = $"{p.score}";
            hintBubble.SetActive(!string.IsNullOrEmpty(p.hint));
            hintText.text = p.hint;
        }

        private void ShowSuccess(Feedback f)
        {
            warningBanner.SetActive(false);
            successTitle.text = f.title;
            successBody.text = f.body;
            successSheet.SetActive(true);
        }

        private void ShowWarning() => warningBanner.SetActive(true);
    }
}
