using System;
using UnityEngine;
using ARCircuitsLab.AR;

namespace ARCircuitsLab.Events
{
    /// <summary>
    /// Observer pattern: one central place for game-wide events.
    /// Managers raise events here and other managers listen,
    /// so no manager needs a direct reference to another.
    /// </summary>
    public static class GameEvents
    {
        // ---- AR placement ----
        public static event Action<PlacementState> PlacementStateChanged;
        public static event Action<Transform> BenchPlaced;
        public static event Action RepositionStarted;

        public static void RaisePlacementStateChanged(PlacementState state) => PlacementStateChanged?.Invoke(state);
        public static void RaiseBenchPlaced(Transform bench) => BenchPlaced?.Invoke(bench);
        public static void RaiseRepositionStarted() => RepositionStarted?.Invoke();
    }
}
