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
                "Wire battery + to the switch, the switch to the bulb, and the bulb back to battery -. Then tap the switch.",
                "The bulb is on!",
                "Closing the switch completed the loop, so current can flow. I = V ÷ R = 3 V ÷ 12 Ω ≈ {0:0.00} A.",
                new[] { "1 battery", "1 switch", "1 bulb", "3 wires" },
                "Current only flows when the path is a complete loop."),

            Make("2 Dim the bulb", GoalType.DimBulb, "Dim the bulb",
                "Build a loop with a battery, a switch, a bulb and a resistor. The bulb should glow less brightly.",
                "Go round in one loop: battery +, switch, bulb, resistor, back to battery -. Then close the switch.",
                "The bulb is dimmer!",
                "The resistor adds 12 Ω, so the total is 24 Ω. Less current flows: 3 V ÷ 24 Ω ≈ {0:0.00} A.",
                new[] { "1 battery", "1 switch", "1 bulb", "1 resistor" },
                "More resistance means less current, so the bulb is dimmer."),

            Make("3 Two bulbs in series", GoalType.SeriesBulbs, "Two bulbs in series",
                "Light two bulbs one after the other in a single loop.",
                "Go round in one loop: battery +, switch, bulb, bulb, back to battery -. There is only one path.",
                "Both bulbs are on!",
                "In series the same current ({0:0.00} A) passes through both bulbs, so each one is dimmer.",
                new[] { "1 battery", "1 switch", "2 bulbs", "4 wires" },
                "In series, the same current flows through every part."),

            Make("4 Two bulbs in parallel", GoalType.ParallelBulbs, "Two bulbs in parallel",
                "Light two bulbs so that both shine at full brightness.",
                "Join the first terminals of both bulbs together, and the second terminals together. Then connect each side to the battery.",
                "Full brightness!",
                "Each bulb gets the full 3 V, so each takes {0:0.00} A. The battery gives the total: {1:0.00} A.",
                new[] { "1 battery", "1 switch", "2 bulbs", "5 wires" },
                "In parallel, each branch gets the full battery voltage."),

            Make("5 Fix the broken circuit", GoalType.FixBrokenCircuit, "Fix the broken circuit",
                "This circuit should work, but the bulb stays dark. Find the broken part and replace it.",
                "Tap the grey bulb to check it. Hold it to remove it, put a new bulb in its spot, then wire the new bulb into the loop.",
                "You fixed it!",
                "The old bulb was broken, so it blocked the loop like an open switch. With a new bulb, {0:0.00} A flows again.",
                new[] { "Ready-made circuit", "1 new bulb", "2 wires" },
                "One broken part can stop current in the whole loop."),
        };

        AssetDatabase.SaveAssets();
        return tasks;
    }

    static TaskData Make(string file, GoalType goal, string title, string text, string hint, string success, string why,
        string[] parts, string learn)
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
        t.parts = parts;
        t.learn = learn;
        EditorUtility.SetDirty(t);
        return t;
    }
}
