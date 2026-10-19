using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ARCircuitsLab.AssetLoading;
using ARCircuitsLab.Events;

namespace ARCircuitsLab.AR
{
    /// <summary>
    /// AR Manager: detects horizontal surfaces, shows a placement preview,
    /// places the lab bench on an AR anchor and allows repositioning.
    /// </summary>
    public class ARPlacementManager : MonoBehaviour
    {
        [Header("AR Foundation")]
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;

        [Header("Placement")]
        [Tooltip("Gives us the bench prefab, loaded through Addressables.")]
        [SerializeField] private AssetManager assetManager;
        [SerializeField] private string benchAssetName = "Lab Bench";
        [SerializeField] private Transform previewIndicator;
        [Tooltip("Smallest surface (m²) we accept for the bench.")]
        [SerializeField] private float minPlaneArea = 0.1f;

        private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();

        private Camera arCamera;
        private Pose previewPose;
        private ARAnchor currentAnchor;
        private GameObject bench;

        public PlacementState State { get; private set; } = PlacementState.Scanning;
        public GameObject Bench => bench;

        private void Awake()
        {
            arCamera = Camera.main;
        }

        private void Start()
        {
            previewIndicator.gameObject.SetActive(false);
            GameEvents.RaisePlacementStateChanged(State);
        }

        private void Update()
        {
            if (State == PlacementState.Placed) return;

            bool foundSurface = TryGetSurfaceAtScreenCentre(out previewPose);

            previewIndicator.gameObject.SetActive(foundSurface);
            if (foundSurface)
            {
                previewIndicator.SetPositionAndRotation(previewPose.position, FacingCamera(previewPose.position));
            }

            SetState(foundSurface ? PlacementState.ReadyToPlace : PlacementState.Scanning);
        }

        /// <summary>Called by the "Place bench" button.</summary>
        public void PlaceBench()
        {
            if (State != PlacementState.ReadyToPlace) return;

            GameObject benchPrefab = assetManager.GetPrefab(benchAssetName);
            if (bench == null && benchPrefab == null)
            {
                Debug.LogWarning("[ARPlacementManager] Lab assets are still loading.");
                return;
            }

            // Create a new anchor so the bench stays fixed in the real world.
            var anchorObject = new GameObject("Bench Anchor");
            anchorObject.transform.SetPositionAndRotation(previewPose.position, FacingCamera(previewPose.position));
            ARAnchor newAnchor = anchorObject.AddComponent<ARAnchor>();

            if (bench == null)
                bench = Instantiate(benchPrefab);

            bench.transform.SetParent(anchorObject.transform, false);
            bench.transform.localPosition = Vector3.zero;
            bench.transform.localRotation = Quaternion.identity;

            // Remove the old anchor (only after the bench has moved off it).
            if (currentAnchor != null) Destroy(currentAnchor.gameObject);
            currentAnchor = newAnchor;

            previewIndicator.gameObject.SetActive(false);
            SetPlaneDetection(false);
            SetState(PlacementState.Placed);
            GameEvents.RaiseBenchPlaced(bench.transform);
        }

        /// <summary>Called by the "Move bench" button: scan again and re-place.</summary>
        public void StartRepositioning()
        {
            if (State != PlacementState.Placed) return;

            SetPlaneDetection(true);
            SetState(PlacementState.Scanning);
            GameEvents.RaiseRepositionStarted();
        }

        /// <summary>Removes the bench and anchor (used when going back to the menu).</summary>
        public void ResetPlacement()
        {
            if (bench != null) Destroy(bench);
            if (currentAnchor != null) Destroy(currentAnchor.gameObject);
            bench = null;
            currentAnchor = null;
            SetPlaneDetection(true);
            SetState(PlacementState.Scanning);
        }

        private void OnDisable()
        {
            if (previewIndicator != null) previewIndicator.gameObject.SetActive(false);
        }

        private bool TryGetSurfaceAtScreenCentre(out Pose pose)
        {
            pose = default;
            var screenCentre = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            if (!raycastManager.Raycast(screenCentre, Hits, TrackableType.PlaneWithinPolygon))
                return false;

            ARRaycastHit hit = Hits[0];
            ARPlane plane = planeManager.GetPlane(hit.trackableId);
            if (plane != null && plane.size.x * plane.size.y < minPlaneArea)
                return false;

            pose = hit.pose;
            return true;
        }

        // Turn the bench so its front faces the student.
        private Quaternion FacingCamera(Vector3 position)
        {
            Vector3 toCamera = arCamera.transform.position - position;
            toCamera.y = 0f;
            return toCamera.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(toCamera) : Quaternion.identity;
        }

        // Plane visualisation is only shown while scanning.
        private void SetPlaneDetection(bool active)
        {
            planeManager.enabled = active;
            foreach (ARPlane plane in planeManager.trackables)
                plane.gameObject.SetActive(active);
        }

        private void SetState(PlacementState newState)
        {
            if (State == newState) return;
            State = newState;
            GameEvents.RaisePlacementStateChanged(State);
        }
    }
}
