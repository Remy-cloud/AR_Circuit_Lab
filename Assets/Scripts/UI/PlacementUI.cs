using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ARCircuitsLab.AR;
using ARCircuitsLab.Events;

namespace ARCircuitsLab.UI
{
    /// <summary>
    /// Shows scanning feedback, placement instructions and placement confirmation.
    /// It only listens to events, so it never controls AR logic directly.
    /// </summary>
    public class PlacementUI : MonoBehaviour
    {
        [SerializeField] private ARPlacementManager placementManager;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private Button placeButton;
        [SerializeField] private Button repositionButton;

        private void OnEnable()
        {
            GameEvents.PlacementStateChanged += Refresh;
            placeButton.onClick.AddListener(placementManager.PlaceBench);
            repositionButton.onClick.AddListener(placementManager.StartRepositioning);
        }

        private void OnDisable()
        {
            GameEvents.PlacementStateChanged -= Refresh;
            placeButton.onClick.RemoveListener(placementManager.PlaceBench);
            repositionButton.onClick.RemoveListener(placementManager.StartRepositioning);
        }

        private void Start() => Refresh(placementManager.State);

        private void Refresh(PlacementState state)
        {
            switch (state)
            {
                case PlacementState.Scanning:
                    instructionText.text = "Move your phone slowly to scan the table";
                    break;
                case PlacementState.ReadyToPlace:
                    instructionText.text = "Surface found! Tap Place bench";
                    break;
                case PlacementState.Placed:
                    instructionText.text = "Bench placed";
                    break;
            }

            placeButton.gameObject.SetActive(state != PlacementState.Placed);
            placeButton.interactable = state == PlacementState.ReadyToPlace;
            repositionButton.gameObject.SetActive(state == PlacementState.Placed);
        }
    }
}
