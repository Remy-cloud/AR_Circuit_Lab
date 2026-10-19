namespace ARCircuitsLab.States
{
    /// <summary>State pattern: each app mode decides what happens when it starts and ends.</summary>
    public interface IAppState
    {
        AppStateId Id { get; }
        void Enter();
        void Tick();
        void Exit();
    }

    /// <summary>Shared base so each state only overrides what it needs.</summary>
    public abstract class AppState : IAppState
    {
        protected readonly AppStateManager App;
        protected AppState(AppStateManager app) => App = app;

        public abstract AppStateId Id { get; }
        public virtual void Enter() { }
        public virtual void Tick() { }
        public virtual void Exit() { }
    }
}
