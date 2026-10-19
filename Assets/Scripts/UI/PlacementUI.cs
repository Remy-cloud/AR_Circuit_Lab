using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ARCircuitsLab.AR;
using ARCircuitsLab.Events;

namespace ARCircuitsLab.UI
{
    /// <summary>
    /// Setup screen: plane detection feedback, placement instructions and
    /// placement confirmation. It reacts to placement events only.
    /// </summary>
    public class PlacementUI : ViewBase
    {
        [SerializeField] private ARPlacementManager placementManager;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button placeButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Image scanStep;
        [SerializeField] private Image placeStep;

        private static readonly Color StepOn = new Color(1f, 0.541f, 0.239f);   // orange
        private static readonly Color StepOff = new Color(0.941f, 0.851f, 0.71f); // border

        private void Awake()
        {
            Bind(placeButton, placementManager.PlaceBench);
            Bind(closeButton, app.GoToMenu);
        }

        private void OnEnable()
        {
            GameEvents.PlacementStateChanged += Refresh;
            Refresh(placementManager.State);
        }

        private void OnDisable() => GameEvents.PlacementStateChanged -= Refresh;

        private void Refresh(PlacementState state)
        {
            bool found = state != PlacementState.Scanning;
            titleText.text = state switch
            {
                PlacementState.Scanning => "Looking for a table",
                PlacementState.ReadyToPlace => "Table found",
                _ => "Lab placed"
            };
            bodyText.text = state switch
            {
                PlacementState.Scanning => "Move your phone slowly over a flat surface.",
                PlacementState.ReadyToPlace => "Aim the yellow ring where you want the lab.",
                _ => "Get ready to build."
            };
            placeButton.interactable = state == PlacementState.ReadyToPlace;
            if (scanStep != null) scanStep.color = StepOn;
            if (placeStep != null) placeStep.color = found ? StepOn : StepOff;
        }
    }
}
