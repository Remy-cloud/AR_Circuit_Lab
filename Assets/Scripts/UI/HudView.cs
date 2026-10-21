using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ARCircuitsLab.Circuit;
using ARCircuitsLab.Data;
using ARCircuitsLab.Events;

namespace ARCircuitsLab.UI
{
    /// <summary>
    /// In-lab HUD: current experiment, live readings, hints and instructions,
    /// the parts drawer, success feedback and the short-circuit warning.
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
        [SerializeField] private Color selectedFill = new Color(1f, 0.945f, 0.81f);
        [SerializeField] private Color selectedEdge = new Color(1f, 0.54f, 0.24f);

        [Header("Feedback")]
        [SerializeField] private GameObject successSheet;
        [SerializeField] private TMP_Text successTitle;
        [SerializeField] private TMP_Text successSubtitle;
        [SerializeField] private TMP_Text successPoints;
        [SerializeField] private TMP_Text successBody;
        [SerializeField] private TMP_Text successNext;
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject warningBanner;
        [SerializeField] private TMP_Text warningPenalty;
        [SerializeField] private Button warningUndoButton;

        private Button[] partButtons;
        private Color[] partFills;
        private Color[] partEdges;
        private string hint = "";
        private string instruction = "";

        private void Awake()
        {
            Bind(pauseButton, app.Pause);
            Bind(batteryButton, () => GameEvents.RaisePartSelected(PartType.Battery));
            Bind(switchButton, () => GameEvents.RaisePartSelected(PartType.Switch));
            Bind(bulbButton, () => GameEvents.RaisePartSelected(PartType.Bulb));
            Bind(resistorButton, () => GameEvents.RaisePartSelected(PartType.Resistor));
            Bind(undoButton, GameEvents.RaiseUndoRequested);
            Bind(continueButton, () =>
            {
                successSheet.SetActive(false);
                RefreshHint();
                GameEvents.RaiseContinueRequested();
            });
            Bind(warningUndoButton, () =>
            {
                warningBanner.SetActive(false);
                GameEvents.RaiseUndoRequested();
            });

            successSheet.SetActive(false);

            // Same order as the PartType enum.
            partButtons = new[] { batteryButton, switchButton, bulbButton, resistorButton };
            partFills = new Color[partButtons.Length];
            partEdges = new Color[partButtons.Length];
            for (int i = 0; i < partButtons.Length; i++)
            {
                partFills[i] = partButtons[i].targetGraphic.color;
                Outline o = partButtons[i].GetComponent<Outline>();
                partEdges[i] = o != null ? o.effectColor : Color.clear;
            }
        }

        private void OnEnable()
        {
            GameEvents.ProgressChanged += ShowProgress;
            GameEvents.FeedbackShown += ShowSuccess;
            GameEvents.ShortCircuit += ShowWarning;
            GameEvents.SelectedPartChanged += HighlightPart;
            GameEvents.InstructionChanged += ShowInstruction;
            GameEvents.TaskStarted += OnTaskStarted;
            warningBanner.SetActive(false);
            ShowProgress(GameEvents.LastProgress);
        }

        private void OnDisable()
        {
            GameEvents.ProgressChanged -= ShowProgress;
            GameEvents.FeedbackShown -= ShowSuccess;
            GameEvents.ShortCircuit -= ShowWarning;
            GameEvents.SelectedPartChanged -= HighlightPart;
            GameEvents.InstructionChanged -= ShowInstruction;
            GameEvents.TaskStarted -= OnTaskStarted;
        }

        private void ShowProgress(SessionProgress p)
        {
            if (p.experimentCount == 0) return;   // nothing sent yet
            experimentText.text = p.freeBuild ? "No tasks, no points" : $"Experiment {p.experimentNumber} of {p.experimentCount}";
            titleText.text = p.title;
            goalText.text = p.goal;
            currentText.text = $"{p.current:0.00} A";
            circuitText.text = p.shortCircuit ? "Short!" : p.circuitClosed ? "Closed" : "Open";
            scoreText.text = p.freeBuild ? "-" : $"{p.score}";
            hint = p.hint ?? "";
            if (!p.shortCircuit) warningBanner.SetActive(false);
            warningPenalty.text = p.freeBuild ? "In a real lab, unplug the battery straight away." : "1 mistake recorded, 10 points lost.";
            RefreshHint();
        }

        private void ShowInstruction(string text)
        {
            instruction = text ?? "";
            RefreshHint();
        }

        // An instruction (what to do right now) is shown instead of the experiment hint.
        private void RefreshHint()
        {
            string text = string.IsNullOrEmpty(instruction) ? hint : instruction;
            // While the well-done card is up, keep the view clear so the circuit can be seen.
            hintBubble.SetActive(!string.IsNullOrEmpty(text) && !successSheet.activeSelf);
            hintText.text = text;
        }

        private void HighlightPart(PartType? selected)
        {
            for (int i = 0; i < partButtons.Length; i++)
            {
                bool on = selected.HasValue && (int)selected.Value == i;
                partButtons[i].targetGraphic.color = on ? selectedFill : partFills[i];
                Outline o = partButtons[i].GetComponent<Outline>();
                if (o != null) o.effectColor = on ? selectedEdge : partEdges[i];
            }
        }

        private void OnTaskStarted(int _)
        {
            successSheet.SetActive(false);
            warningBanner.SetActive(false);
        }

        private void ShowSuccess(Feedback f)
        {
            warningBanner.SetActive(false);
            successTitle.text = f.title;
            successBody.text = f.body;
            if (successSubtitle != null && !string.IsNullOrEmpty(f.subtitle)) successSubtitle.text = f.subtitle;
            if (successPoints != null && !string.IsNullOrEmpty(f.points)) successPoints.text = f.points;
            if (successNext != null)
            {
                successNext.gameObject.SetActive(!string.IsNullOrEmpty(f.next));
                successNext.text = f.next;
            }
            var label = continueButton.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = string.IsNullOrEmpty(f.button) ? "Next experiment" : f.button;
            successSheet.SetActive(true);
            RefreshHint();
        }

        private void ShowWarning() => warningBanner.SetActive(true);
    }
}
