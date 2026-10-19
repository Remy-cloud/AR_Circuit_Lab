using UnityEngine;
using ARCircuitsLab.Data;
using ARCircuitsLab.Events;

namespace ARCircuitsLab.States
{
    /// <summary>Main menu: nothing AR is running and lab models are released from memory.</summary>
    public class MenuState : AppState
    {
        public MenuState(AppStateManager app) : base(app) { }
        public override AppStateId Id => AppStateId.Menu;

        public override void Enter()
        {
            App.Placement.ResetPlacement();
            App.Placement.gameObject.SetActive(false);
            App.Assets.ReleaseLabAssets();
            App.UI.Show(Id);
        }
    }

    /// <summary>Briefing: explains the experiment before the camera opens.</summary>
    public class BriefingState : AppState
    {
        public BriefingState(AppStateManager app) : base(app) { }
        public override AppStateId Id => AppStateId.Briefing;
        public override void Enter() => App.UI.Show(Id);
    }

    /// <summary>AR setup: load lab models, scan for a surface, wait for the bench to be placed.</summary>
    public class SetupState : AppState
    {
        public SetupState(AppStateManager app) : base(app) { }
        public override AppStateId Id => AppStateId.Setup;

        public override void Enter()
        {
            App.UI.Show(Id);
            _ = App.Assets.LoadLabAssetsAsync();
            App.Placement.gameObject.SetActive(true);
            GameEvents.BenchPlaced += OnBenchPlaced;
        }

        public override void Exit() => GameEvents.BenchPlaced -= OnBenchPlaced;

        private void OnBenchPlaced(Transform _) => App.ChangeState(AppStateId.Simulation);
    }

    /// <summary>The lab itself: tasks run until the session ends.</summary>
    public class SimulationState : AppState
    {
        public SimulationState(AppStateManager app) : base(app) { }
        public override AppStateId Id => AppStateId.Simulation;

        public override void Enter()
        {
            App.UI.Show(Id);
            GameEvents.SessionEnded += OnSessionEnded;
        }

        public override void Exit() => GameEvents.SessionEnded -= OnSessionEnded;

        private void OnSessionEnded(SessionRecord _) => App.ChangeState(AppStateId.Summary);
    }

    /// <summary>Paused: time stops, so the task timer and electrons freeze.</summary>
    public class PausedState : AppState
    {
        public PausedState(AppStateManager app) : base(app) { }
        public override AppStateId Id => AppStateId.Paused;

        public override void Enter() { Time.timeScale = 0f; App.UI.Show(Id); }
        public override void Exit() => Time.timeScale = 1f;
    }

    /// <summary>Summary: shows the outcome of the session that just ended.</summary>
    public class SummaryState : AppState
    {
        public SummaryState(AppStateManager app) : base(app) { }
        public override AppStateId Id => AppStateId.Summary;
        public override void Enter() => App.UI.Show(Id);
    }

    /// <summary>History: lists the most recent saved sessions.</summary>
    public class HistoryState : AppState
    {
        public HistoryState(AppStateManager app) : base(app) { }
        public override AppStateId Id => AppStateId.History;
        public override void Enter() => App.UI.Show(Id);
    }
}
