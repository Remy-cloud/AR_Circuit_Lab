using UnityEngine;

namespace ARCircuitsLab.Tasks
{
    /// <summary>What the student has to build to finish an experiment.</summary>
    public enum GoalType { LightBulb, DimBulb, SeriesBulbs, ParallelBulbs, FixBrokenCircuit }

    /// <summary>
    /// One experiment (configuration data). Text and points can be edited in the
    /// Inspector without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "AR Circuits Lab/Task Data", fileName = "TaskData")]
    public class TaskData : ScriptableObject
    {
        public string title = "Experiment";
        [TextArea] public string goal;
        [TextArea] public string hint;
        public GoalType goalType;
        public int points = 100;

        [Header("When finished")]
        public string successTitle = "Well done!";
        [Tooltip("{0} = current through the bulb (A), {1} = current from the battery (A).")]
        [TextArea] public string explanation;
    }
}
