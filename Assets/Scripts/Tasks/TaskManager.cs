using UnityEngine;
using ARCircuitsLab.Circuit;
using ARCircuitsLab.Data;
using ARCircuitsLab.Events;
using ARCircuitsLab.States;

namespace ARCircuitsLab.Tasks
{
    /// <summary>
    /// Task Manager: runs the lab session. It gives the student one experiment at a time,
    /// checks the circuit after every change, keeps score, mistakes and time,
    /// and ends the session with a SessionRecord. In Free build it only reports readings.
    /// </summary>
    public class TaskManager : MonoBehaviour
    {
        [SerializeField] private CircuitManager circuit;
        [SerializeField] private TaskData[] tasks;
        [SerializeField] private int mistakePenalty = 10;

        private LabMode mode;
        private bool running;
        private bool waitingForNext;
        private int index;
        private int score;
        private int mistakes;
        private int completed;
        private float timeTaken;
        private AppStateId state;

        public LabMode Mode => mode;
        public int TaskCount => tasks.Length;
        public TaskData Current => tasks[index];

        private void OnEnable()
        {
            GameEvents.BenchPlaced += OnBenchPlaced;
            GameEvents.CircuitUpdated += OnCircuitUpdated;
            GameEvents.ShortCircuit += OnShortCircuit;
            GameEvents.RestartRequested += Restart;
            GameEvents.ContinueRequested += Continue;
            GameEvents.AppStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.BenchPlaced -= OnBenchPlaced;
            GameEvents.CircuitUpdated -= OnCircuitUpdated;
            GameEvents.ShortCircuit -= OnShortCircuit;
            GameEvents.RestartRequested -= Restart;
            GameEvents.ContinueRequested -= Continue;
            GameEvents.AppStateChanged -= OnStateChanged;
        }

        private void Update()
        {
            if (running && state == AppStateId.Simulation) timeTaken += Time.deltaTime;
        }

        /// <summary>Chosen on the home screen, before the bench is placed.</summary>
        public void SetMode(LabMode newMode) => mode = newMode;

        // ------------------------------------------------------------ session

        private void OnBenchPlaced(Transform bench)
        {
            circuit.AttachToBench(bench);       // does nothing if the bench was only moved
            if (!running) StartSession();
        }

        private void StartSession()
        {
            running = true;
            score = mistakes = completed = 0;
            timeTaken = 0f;
            circuit.ClearCircuit();

            if (mode == LabMode.FreeBuild) Publish();
            else BeginTask(0);
        }

        private void Restart()
        {
            if (!circuit.IsReady) return;
            StartSession();
        }

        private void BeginTask(int i)
        {
            index = i;
            waitingForNext = false;
            if (Current.goalType == GoalType.FixBrokenCircuit) BuildBrokenCircuit();
            GameEvents.RaiseTaskStarted(index);
            Publish();
        }

        private void Continue()
        {
            if (!running || !waitingForNext) return;
            if (index + 1 < tasks.Length) BeginTask(index + 1);
            else EndSession();
        }

        private void EndSession()
        {
            running = false;
            waitingForNext = false;
            var record = new SessionRecord(Mathf.Max(0, score), timeTaken, completed, mistakes);
            GameEvents.RaiseSessionEnded(record);
        }

        private void OnStateChanged(AppStateId newState)
        {
            state = newState;
            if (newState != AppStateId.Menu) return;
            running = false;        // the bench is gone; start fresh next time
            circuit.Detach();
        }

        // ------------------------------------------------------------ checking

        private void OnCircuitUpdated()
        {
            if (!running) return;
            Publish();
            if (mode == LabMode.Experiments && !waitingForNext && IsGoalMet(Current.goalType))
                CompleteTask();
        }

        private void OnShortCircuit()
        {
            if (!running || mode == LabMode.FreeBuild) return;
            mistakes++;
            score -= mistakePenalty;
            Publish();
        }

        private bool IsGoalMet(GoalType goal)
        {
            if (circuit.IsShortCircuit) return false;
            switch (goal)
            {
                case GoalType.LightBulb:
                    return circuit.Find<Bulb>(b => b.IsLit) != null
                        && circuit.Find<Switch>(s => s.IsOn && Mathf.Abs(s.Current) > 0.01f) != null;

                case GoalType.DimBulb:
                    return circuit.Find<Bulb>(b => b.IsLit && b.Brightness < 0.6f) != null
                        && circuit.Find<Resistor>(r => Mathf.Abs(r.Current) > 0.01f) != null;

                case GoalType.SeriesBulbs:
                    return circuit.Count<Bulb>(b => b.IsLit && b.Brightness < 0.6f) >= 2 && SameCurrentInTwoBulbs();

                case GoalType.ParallelBulbs:
                    return circuit.Count<Bulb>(b => b.Brightness > 0.8f) >= 2;

                case GoalType.FixBrokenCircuit:
                    return circuit.Find<Bulb>(b => b.IsWorking && b.IsLit) != null;
            }
            return false;
        }

        // In series the same current passes through every bulb.
        private bool SameCurrentInTwoBulbs()
        {
            Bulb first = null;
            foreach (CircuitComponent p in circuit.Parts)
            {
                if (!(p is Bulb b) || !b.IsLit) continue;
                if (first == null) { first = b; continue; }
                if (Mathf.Abs(Mathf.Abs(first.Current) - Mathf.Abs(b.Current)) < 0.01f) return true;
            }
            return false;
        }

        private void CompleteTask()
        {
            waitingForNext = true;
            completed++;
            score += Current.points;

            Bulb bulb = circuit.Find<Bulb>(b => b.IsLit);
            float bulbCurrent = bulb != null ? Mathf.Abs(bulb.Current) : 0f;
            bool last = index == tasks.Length - 1;

            GameEvents.RaiseTaskCompleted(index);
            GameEvents.RaiseFeedbackShown(new Feedback
            {
                title = Current.successTitle,
                body = string.Format(Current.explanation, bulbCurrent, circuit.BatteryCurrent),
                subtitle = $"Experiment {index + 1} complete",
                points = $"+{Current.points}",
                button = last ? "See my results" : "Next experiment",
            });
            Publish();
        }

        // Experiment 5: a ready-made loop with a broken bulb. The student has to find and replace it.
        private void BuildBrokenCircuit()
        {
            circuit.ClearCircuit();
            CircuitComponent battery = circuit.Place(PartType.Battery, 5, record: false);
            var sw = (Switch)circuit.Place(PartType.Switch, 1, record: false);
            CircuitComponent bulb = circuit.Place(PartType.Bulb, 2, working: false, record: false);
            sw.SetOn(true);
            circuit.Connect(battery.B, sw.A, false);
            circuit.Connect(sw.B, bulb.A, false);
            circuit.Connect(bulb.B, battery.A, false);
            circuit.ClearUndoHistory();
        }

        private void Publish()
        {
            var p = new SessionProgress
            {
                freeBuild = mode == LabMode.FreeBuild,
                experimentCount = tasks.Length,
                score = score,
                mistakes = mistakes,
                current = circuit.BatteryCurrent,
                circuitClosed = circuit.IsClosed,
                shortCircuit = circuit.IsShortCircuit,
            };
            if (p.freeBuild)
            {
                p.title = "Free build";
                p.goal = "Build any circuit you like. No points, nothing to finish.";
                p.hint = "Tap a part, then a glowing spot. Tap two terminals to add a wire. Hold to remove.";
            }
            else
            {
                p.experimentNumber = index + 1;
                p.title = Current.title;
                p.goal = Current.goal;
                p.hint = Current.hint;
            }
            GameEvents.RaiseProgressChanged(p);
        }
    }
}
