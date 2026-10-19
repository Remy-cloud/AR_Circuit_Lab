using System;
using UnityEngine;
using ARCircuitsLab.AR;
using ARCircuitsLab.Circuit;
using ARCircuitsLab.Data;
using ARCircuitsLab.States;

namespace ARCircuitsLab.Events
{
    /// <summary>
    /// Observer pattern: one central place for game-wide events.
    /// Managers raise events here and other managers listen,
    /// so no manager needs a direct reference to another.
    /// </summary>
    public static class GameEvents
    {
        // ---- App flow ----
        public static event Action<AppStateId> AppStateChanged;
        public static event Action ButtonClicked;

        // ---- Assets ----
        public static event Action CoreAssetsLoaded;
        public static event Action LabAssetsLoaded;
        public static event Action LabAssetsReleased;

        // ---- AR placement ----
        public static event Action<PlacementState> PlacementStateChanged;
        public static event Action<Transform> BenchPlaced;
        public static event Action RepositionStarted;

        // ---- Circuit (used from Phase 3) ----
        public static event Action ComponentPlaced;
        public static event Action WireConnected;
        public static event Action<bool> SwitchToggled;
        public static event Action CircuitUpdated;
        public static event Action ShortCircuit;

        // ---- Student input ----
        public static event Action<PartType> PartSelected;
        public static event Action UndoRequested;
        public static event Action RestartRequested;

        // ---- Tasks & session ----
        public static event Action<SessionProgress> ProgressChanged;
        public static event Action<Feedback> FeedbackShown;
        /// <summary>The last progress sent, so a screen that opens later can show it straight away.</summary>
        public static SessionProgress LastProgress { get; private set; }
        public static event Action<int> TaskStarted;
        public static event Action<int> TaskCompleted;
        public static event Action<SessionRecord> SessionEnded;

        public static void RaiseAppStateChanged(AppStateId s) => AppStateChanged?.Invoke(s);
        public static void RaiseButtonClicked() => ButtonClicked?.Invoke();
        public static void RaiseCoreAssetsLoaded() => CoreAssetsLoaded?.Invoke();
        public static void RaiseLabAssetsLoaded() => LabAssetsLoaded?.Invoke();
        public static void RaiseLabAssetsReleased() => LabAssetsReleased?.Invoke();
        public static void RaisePlacementStateChanged(PlacementState s) => PlacementStateChanged?.Invoke(s);
        public static void RaiseBenchPlaced(Transform bench) => BenchPlaced?.Invoke(bench);
        public static void RaiseRepositionStarted() => RepositionStarted?.Invoke();
        public static void RaiseComponentPlaced() => ComponentPlaced?.Invoke();
        public static void RaiseWireConnected() => WireConnected?.Invoke();
        public static void RaiseSwitchToggled(bool isOn) => SwitchToggled?.Invoke(isOn);
        public static void RaiseCircuitUpdated() => CircuitUpdated?.Invoke();
        public static void RaiseShortCircuit() => ShortCircuit?.Invoke();
        public static void RaisePartSelected(PartType p) => PartSelected?.Invoke(p);
        public static void RaiseUndoRequested() => UndoRequested?.Invoke();
        public static void RaiseRestartRequested() => RestartRequested?.Invoke();
        public static void RaiseProgressChanged(SessionProgress p) { LastProgress = p; ProgressChanged?.Invoke(p); }
        public static void RaiseFeedbackShown(Feedback f) => FeedbackShown?.Invoke(f);
        public static void RaiseTaskStarted(int index) => TaskStarted?.Invoke(index);
        public static void RaiseTaskCompleted(int index) => TaskCompleted?.Invoke(index);
        public static void RaiseSessionEnded(SessionRecord r) => SessionEnded?.Invoke(r);
    }
}
