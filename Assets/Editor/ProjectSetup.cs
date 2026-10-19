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

        if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait)
        { PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait; changed = true; }

        if (changed)
        {
            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectSetup] iOS player settings applied.");
        }
    }
}
