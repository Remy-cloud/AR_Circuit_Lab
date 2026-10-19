using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

/// <summary>
/// Puts our assets into Addressables groups with labels:
///   "Lab Content" group, label "lab"   → bench + circuit part prefabs
///   "Audio"       group, label "audio" → all sound effects
/// Menu: AR Circuits Lab > Setup Addressables
/// </summary>
public static class AddressablesSetup
{
    [MenuItem("AR Circuits Lab/Setup Addressables")]
    public static void Setup()
    {
        var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        settings.AddLabel("lab");
        settings.AddLabel("audio");

        AddressableAssetGroup labGroup = GetOrCreateGroup(settings, "Lab Content");
        AddressableAssetGroup audioGroup = GetOrCreateGroup(settings, "Audio");

        int lab = AddFolder(settings, labGroup, "Assets/Prefabs", "t:Prefab", "lab",
            skip: new[] { "AR Plane", "Placement Indicator" }); // these stay in the scene
        int audio = AddFolder(settings, audioGroup, "Assets/Audio", "t:AudioClip", "audio", skip: null);

        AssetDatabase.SaveAssets();
        Debug.Log($"[AddressablesSetup] {lab} lab prefabs and {audio} audio clips are Addressable.");
    }

    static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string name)
    {
        return settings.FindGroup(name) ?? settings.CreateGroup(name, false, false, true, null,
            typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
    }

    static int AddFolder(AddressableAssetSettings settings, AddressableAssetGroup group,
        string folder, string filter, string label, string[] skip)
    {
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets(filter, new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (skip != null && System.Array.IndexOf(skip, name) >= 0) continue;

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = name;
            entry.SetLabel(label, true, true);
            count++;
        }
        return count;
    }
}
