using System.IO;
using UnityEditor;
using UnityEngine;
using ARCircuitsLab.Data;

/// <summary>
/// Quick check that saving and loading works, without running the app.
/// Writes to a separate test file so real history is not affected.
/// Menu: AR Circuits Lab > Test Save and Load Sessions
/// </summary>
public static class SessionStorageTest
{
    [MenuItem("AR Circuits Lab/Test Save and Load Sessions")]
    public static void Run()
    {
        string path = Path.Combine(Application.persistentDataPath, "sessions_test.json");
        if (File.Exists(path)) File.Delete(path);

        var repo = new SessionRepository(path, 20);
        for (int i = 1; i <= 7; i++)
            repo.Save(new SessionRecord(score: 100 * i, timeTaken: 60f * i, tasksCompleted: 3, mistakes: i % 3));

        var reloaded = new SessionRepository(path, 20);   // simulates a new app launch
        reloaded.Load();

        Debug.Log($"[SessionStorageTest] File: {path}");
        Debug.Log($"[SessionStorageTest] Saved 7, reloaded {reloaded.All.Count}. Five most recent:");
        foreach (SessionRecord r in reloaded.GetRecent(5))
            Debug.Log($"   score {r.score}, {r.timeTaken:0}s, {r.tasksCompleted} tasks, {r.mistakes} mistakes, {r.date}");
    }
}
