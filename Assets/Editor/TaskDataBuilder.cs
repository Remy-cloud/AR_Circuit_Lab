using UnityEditor;
using UnityEngine;
using ARCircuitsLab.Tasks;

/// <summary>
/// Creates the five experiment assets (TaskData) in Assets/Data/Tasks.
/// The text can then be edited in the Inspector.
/// Menu: AR Circuits Lab > Create Experiments
/// </summary>
public static class TaskDataBuilder
{
    const string Folder = "Assets/Data/Tasks";

    [MenuItem("AR Circuits Lab/Create Experiments")]
    public static TaskData[] CreateAll()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data")) AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Data", "Tasks");

        var tasks = new[]
        {
            Make("1 Light the bulb", GoalType.LightBulb, "Light the bulb",
                "Connect a battery, a switch and a bulb in one loop. Then close the switch.",
                "Wire battery + to the switch, the switch to the bulb, and the bulb back to battery -.",
                "The bulb is on!",
                "Closing the switch completed the loop, so current can flow. I = V ÷ R = 3 V ÷ 12 Ω ≈ {0:0.00} A."),

            Make("2 Dim the bulb", GoalType.DimBulb, "Dim the bulb",
                "Add a resistor into the loop so the bulb glows less brightly.",
                "Press and hold a terminal to remove its wire. Put the resistor in the gap and wire it in.",
                "The bulb is dimmer!",
                "The resistor adds 12 Ω, so the total is 24 Ω. Less current flows: 3 V ÷ 24 Ω ≈ {0:0.00} A."),

            Make("3 Two bulbs in series", GoalType.SeriesBulbs, "Two bulbs in series",
                "Light two bulbs one after the other in a single loop.",
                "Press and hold the resistor to remove it. Put a second bulb in its place, so there is only one path.",
                "Both bulbs are on!",
                "In series the same current ({0:0.00} A) passes through both bulbs, so each one is dimmer."),

            Make("4 Two bulbs in parallel", GoalType.ParallelBulbs, "Two bulbs in parallel",
                "Light two bulbs so that both shine at full brightness.",
                "Join the first terminals of both bulbs together, and the second terminals together. Each bulb gets its own path.",
                "Full brightness!",
                "Each bulb gets the full 3 V, so each takes {0:0.00} A. The battery gives the total: {1:0.00} A."),

            Make("5 Fix the broken circuit", GoalType.FixBrokenCircuit, "Fix the broken circuit",
                "This circuit should work, but the bulb stays dark. Find the fault and fix it.",
                "Tap a part to check it. Press and hold a broken part to remove it, then build the loop again.",
                "You fixed it!",
                "The old bulb was broken, so it blocked the loop like an open switch. With a new bulb, {0:0.00} A flows again."),
        };

        AssetDatabase.SaveAssets();
        return tasks;
    }

    static TaskData Make(string file, GoalType goal, string title, string text, string hint, string success, string why)
    {
        string path = $"{Folder}/{file}.asset";
        var t = AssetDatabase.LoadAssetAtPath<TaskData>(path);
        if (t == null)
        {
            t = ScriptableObject.CreateInstance<TaskData>();
            AssetDatabase.CreateAsset(t, path);
        }
        t.title = title;
        t.goal = text;
        t.hint = hint;
        t.goalType = goal;
        t.points = 100;
        t.successTitle = success;
        t.explanation = why;
        EditorUtility.SetDirty(t);
        return t;
    }
}
