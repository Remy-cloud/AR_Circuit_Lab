using System.Collections.Generic;
using UnityEngine;
using ARCircuitsLab.AR;
using ARCircuitsLab.AssetLoading;
using ARCircuitsLab.Events;
using ARCircuitsLab.Managers;
using ARCircuitsLab.Tasks;
using ARCircuitsLab.UI;

namespace ARCircuitsLab.States
{
    /// <summary>
    /// Runs the app flow Start → Setup → Simulation → Summary using the State pattern.
    /// Only one state is active at a time; switching calls Exit() then Enter().
    /// </summary>
    public class AppStateManager : MonoBehaviour
    {
        [SerializeField] private AppStateId startState = AppStateId.Menu;
        [SerializeField] private AssetManager assets;
        [SerializeField] private ARPlacementManager placement;
        [SerializeField] private UIManager ui;
        [SerializeField] private DataManager data;
        [SerializeField] private TaskManager tasks;

        private readonly Dictionary<AppStateId, IAppState> states = new Dictionary<AppStateId, IAppState>();

        public AssetManager Assets => assets;
        public ARPlacementManager Placement => placement;
        public UIManager UI => ui;
        public DataManager Data => data;
        public LabMode Mode => tasks.Mode;
        public IAppState Current { get; private set; }

        private void Awake()
        {
            Register(new MenuState(this));
            Register(new BriefingState(this));
            Register(new SetupState(this));
            Register(new SimulationState(this));
            Register(new PausedState(this));
            Register(new SummaryState(this));
            Register(new HistoryState(this));
        }

        private void Start() => ChangeState(startState);
        private void Update() => Current?.Tick();

        public void ChangeState(AppStateId id)
        {
            if (Current != null && Current.Id == id) return;
            Current?.Exit();
            Current = states[id];
            Current.Enter();
            GameEvents.RaiseAppStateChanged(id);
            Debug.Log($"[AppStateManager] State → {id}");
        }

        // ---- Called by UI buttons ----
        public void ShowBriefing() => ChangeState(AppStateId.Briefing);
        public void StartLab() => StartLab(LabMode.Experiments);
        public void StartFreeBuild() => StartLab(LabMode.FreeBuild);
        public void GoToMenu() => ChangeState(AppStateId.Menu);
        public void Pause() => ChangeState(AppStateId.Paused);
        public void Resume() => ChangeState(AppStateId.Simulation);
        public void ShowHistory() => ChangeState(AppStateId.History);

        /// <summary>Scan again and move the bench, keeping the circuit on it.</summary>
        public void MoveBench()
        {
            placement.StartRepositioning();
            ChangeState(AppStateId.Setup);
        }

        /// <summary>Start the experiments again on the same bench.</summary>
        public void RestartLab()
        {
            GameEvents.RaiseRestartRequested();
            ChangeState(AppStateId.Simulation);
        }

        private void StartLab(LabMode mode)
        {
            tasks.SetMode(mode);
            ChangeState(AppStateId.Setup);
        }

        private void Register(IAppState state) => states[state.Id] = state;
    }
}
