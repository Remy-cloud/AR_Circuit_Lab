using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// One-click iPhone build: builds the Addressables content first,
/// then exports the Xcode project to the Builds/iOS folder.
/// Menu: AR Circuits Lab > Build iPhone (Xcode project)
/// </summary>
public static class BuildTools
{
    const string OutputFolder = "Builds/iOS";

    [MenuItem("AR Circuits Lab/Build iPhone (Xcode project)")]
    public static void BuildIPhone()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
        {
            Debug.LogError("[BuildTools] Switch the platform to iOS first (File > Build Profiles).");
            return;
        }

        ProjectSetup.Apply();
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;   // Xcode signs with your Apple ID

        // 1. Addressables: the lab models and sounds must be built before the player.
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult content);
        if (!string.IsNullOrEmpty(content.Error))
        {
            Debug.LogError("[BuildTools] Addressables build failed: " + content.Error);
            return;
        }

        // 2. The app itself, as an Xcode project.
        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = OutputFolder,
            target = BuildTarget.iOS,
            options = BuildOptions.None,
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildTools] iPhone build done in {report.summary.totalTime.TotalSeconds:0}s → {OutputFolder}/Unity-iPhone.xcodeproj");
            OpenInXcode();
        }
        else
            Debug.LogError($"[BuildTools] iPhone build {report.summary.result} with {report.summary.totalErrors} errors.");
    }

    /// <summary>Opens the exported project in Xcode, ready to press Run.</summary>
    [MenuItem("AR Circuits Lab/Open iPhone Build in Xcode")]
    public static void OpenInXcode()
    {
        string project = System.IO.Path.GetFullPath(OutputFolder + "/Unity-iPhone.xcodeproj");
        if (!System.IO.Directory.Exists(project)) { Debug.LogError("[BuildTools] Build the iPhone project first."); return; }
        System.Diagnostics.Process.Start("open", "\"" + project + "\"");
    }
}
