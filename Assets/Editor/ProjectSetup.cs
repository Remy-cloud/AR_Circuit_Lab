using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

// Applies the app's iOS player settings automatically when Unity compiles.
// Runs safely every time: it only changes values that are not already correct.
[InitializeOnLoad]
public static class ProjectSetup
{
    const string Company = "Remy Intwali";
    const string Product = "AR Circuits Lab";
    const string BundleId = "com.remy.arcircuitslab";
    const string CameraText = "Used to place the circuit lab on your table";
    const string AppleTeamId = "6G2ZXGK743";   // Personal Team, so Xcode signs every new build
    const string IconPath = "Assets/UI/AppIcon/AppIcon.png";

    static ProjectSetup()
    {
        EditorApplication.delayCall += Apply;
    }

    [MenuItem("AR Circuits Lab/Apply Project Settings")]
    public static void Apply()
    {
        bool changed = false;

        if (PlayerSettings.companyName != Company) { PlayerSettings.companyName = Company; changed = true; }
        if (PlayerSettings.productName != Product) { PlayerSettings.productName = Product; changed = true; }

        if (PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS) != BundleId)
        { PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId); changed = true; }

        if (PlayerSettings.iOS.cameraUsageDescription != CameraText)
        { PlayerSettings.iOS.cameraUsageDescription = CameraText; changed = true; }

        if (PlayerSettings.iOS.appleDeveloperTeamID != AppleTeamId)
        { PlayerSettings.iOS.appleDeveloperTeamID = AppleTeamId; changed = true; }

        // App icon: one 1024 x 1024 image; Unity makes every size the iPhone needs from it.
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        Texture2D[] icons = PlayerSettings.GetIcons(NamedBuildTarget.Unknown, IconKind.Any);
        if (icon != null && (icons.Length == 0 || icons[0] != icon))
        { PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any); changed = true; }

        if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait)
        { PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait; changed = true; }

        if (changed)
        {
            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectSetup] iOS player settings applied.");
        }
    }
}
