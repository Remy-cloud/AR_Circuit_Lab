using UnityEditor;
using UnityEngine;
using ARCircuitsLab.AR;
using ARCircuitsLab.Data;
using ARCircuitsLab.Events;
using ARCircuitsLab.States;

/// <summary>
/// Play-mode shortcuts for testing screens before the circuit simulation exists.
/// Menu: AR Circuits Lab > Debug
/// </summary>
public static class DebugTools
{
    static AppStateManager App => Object.FindFirstObjectByType<AppStateManager>();

    // The simulator needs a right-mouse drag to look at a table, so this puts the bench
    // just in front of the camera instead. Use it on the "Looking for a table" screen.
    [MenuItem("AR Circuits Lab/Debug/Place Bench In Front")]
    static void PlaceBenchInFront()
    {
        var placement = Object.FindFirstObjectByType<ARPlacementManager>();
        Transform cam = Camera.main.transform;
        Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;

        // Tilt the view down like a phone held over a desk (the camera's parent turns, so it stays tilted).
        Transform offset = cam.parent;
        float pitch = Vector3.SignedAngle(forward, cam.forward, cam.right);
        offset.RotateAround(cam.position, cam.right, 50f - pitch);

        placement.PlaceBenchAt(cam.position + forward * 0.32f + Vector3.down * 0.38f);
    }

    [MenuItem("AR Circuits Lab/Debug/Place Bench In Front", true)]
    static bool CanPlaceBench() => Application.isPlaying;

    [MenuItem("AR Circuits Lab/Debug/Skip to Lab HUD")]
    static void SkipToHud()
    {
        App.ChangeState(AppStateId.Simulation);
        GameEvents.RaiseProgressChanged(new SessionProgress
        {
            experimentNumber = 1, experimentCount = 5, title = "Light the bulb",
            goal = "Connect a battery, a switch and a bulb in one loop.",
            hint = "Tap a part below, then tap the bench.", score = 0, current = 0f
        });
    }

    [MenuItem("AR Circuits Lab/Debug/Show Experiment Complete")]
    static void Success() => GameEvents.RaiseFeedbackShown(new Feedback
    {
        title = "The bulb is on!",
        body = "Closing the switch completed the loop, so current can flow. 3 V across the bulb gives 0.25 A."
    });

    [MenuItem("AR Circuits Lab/Debug/Show Short Circuit")]
    static void Short() => GameEvents.RaiseShortCircuit();

    [MenuItem("AR Circuits Lab/Debug/Finish Session")]
    static void Finish()
    {
        App.ChangeState(AppStateId.Simulation);
        GameEvents.RaiseSessionEnded(new SessionRecord(
            score: Random.Range(150, 301), timeTaken: Random.Range(300f, 700f),
            tasksCompleted: 3, mistakes: Random.Range(0, 5)));
    }

    [MenuItem("AR Circuits Lab/Debug/Skip to Lab HUD", true)]
    [MenuItem("AR Circuits Lab/Debug/Show Experiment Complete", true)]
    [MenuItem("AR Circuits Lab/Debug/Show Short Circuit", true)]
    [MenuItem("AR Circuits Lab/Debug/Finish Session", true)]
    static bool OnlyInPlayMode() => Application.isPlaying;
}
