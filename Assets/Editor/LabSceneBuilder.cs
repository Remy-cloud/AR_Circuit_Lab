using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ARCircuitsLab.AR;
using ARCircuitsLab.AssetLoading;
using ARCircuitsLab.Audio;
using ARCircuitsLab.Managers;
using ARCircuitsLab.States;
using ARCircuitsLab.UI;

/// <summary>
/// Builds the "Lab" scene: AR Session, XR Origin with plane detection,
/// raycasting and anchors, the placement manager and the setup UI.
/// Menu: AR Circuits Lab > Create Lab Scene
/// </summary>
public static class LabSceneBuilder
{
    const string ScenePath = "Assets/Scenes/Lab.unity";
    const string PrefabFolder = "Assets/Prefabs";
    const string MaterialFolder = "Assets/Materials";

    [MenuItem("AR Circuits Lab/Create Lab Scene")]
    public static void CreateLabScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateLight();
        CreateARSession();
        XROrigin origin = CreateXROrigin();

        // AR Foundation managers live on the XR Origin.
        var planeManager = origin.gameObject.AddComponent<ARPlaneManager>();
        planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        planeManager.planePrefab = CreatePlanePrefab();
        var raycastManager = origin.gameObject.AddComponent<ARRaycastManager>();
        origin.gameObject.AddComponent<ARAnchorManager>();

        // Managers (each one has a single job).
        var managers = new GameObject("Managers");
        var assetManager = AddManager<AssetManager>(managers, "Asset Manager");
        var dataManager = AddManager<DataManager>(managers, "Data Manager");
        var audioManager = AddManager<AudioManager>(managers, "Audio Manager");
        Wire(audioManager, "assetManager", assetManager);
        var placement = AddManager<ARPlacementManager>(managers, "AR Placement Manager");

        GameObject indicator = (GameObject)PrefabUtility.InstantiatePrefab(CreateIndicatorPrefab());
        indicator.name = "Placement Indicator";
        CreateBenchPrefab();

        var so = new SerializedObject(placement);
        so.FindProperty("raycastManager").objectReferenceValue = raycastManager;
        so.FindProperty("planeManager").objectReferenceValue = planeManager;
        so.FindProperty("assetManager").objectReferenceValue = assetManager;
        so.FindProperty("previewIndicator").objectReferenceValue = indicator.transform;
        so.ApplyModifiedPropertiesWithoutUndo();

        var uiManager = AddManager<UIManager>(managers, "UI Manager");
        var stateManager = AddManager<AppStateManager>(managers, "App State Manager");
        var stSo = new SerializedObject(stateManager);
        stSo.FindProperty("startState").enumValueIndex = (int)AppStateId.Menu;
        stSo.FindProperty("assets").objectReferenceValue = assetManager;
        stSo.FindProperty("placement").objectReferenceValue = placement;
        stSo.FindProperty("ui").objectReferenceValue = uiManager;
        stSo.FindProperty("data").objectReferenceValue = dataManager;
        stSo.ApplyModifiedPropertiesWithoutUndo();

        // All screens, then tell the UI Manager which screen belongs to which state.
        LabUIBuilder.Screens ui = LabUIBuilder.Build(stateManager, placement);
        var uiSo = new SerializedObject(uiManager);
        var list = uiSo.FindProperty("screens");
        var map = new (AppStateId, GameObject)[]
        {
            (AppStateId.Menu, ui.home), (AppStateId.Briefing, ui.briefing), (AppStateId.Setup, ui.setup),
            (AppStateId.Simulation, ui.hud), (AppStateId.Paused, ui.hud), (AppStateId.Paused, ui.pause),
            (AppStateId.Summary, ui.summary), (AppStateId.History, ui.history),
        };
        list.arraySize = map.Length;
        for (int i = 0; i < map.Length; i++) SetScreen(list.GetArrayElementAtIndex(i), map[i].Item1, map[i].Item2);
        uiSo.ApplyModifiedPropertiesWithoutUndo();
        foreach (var m in map) m.Item2.SetActive(false);

        AddressablesSetup.Setup();

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Debug.Log("[LabSceneBuilder] Lab scene created at " + ScenePath);
    }

    // ---------- helpers ----------

    static T AddManager<T>(GameObject parent, string name) where T : Component
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        return go.AddComponent<T>();
    }

    static void Wire(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetScreen(SerializedProperty entry, AppStateId state, GameObject panel)
    {
        entry.FindPropertyRelative("state").enumValueIndex = (int)state;
        entry.FindPropertyRelative("panel").objectReferenceValue = panel;
    }

    // ---------- scene objects ----------

    static void CreateLight()
    {
        var light = new GameObject("Directional Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    static void CreateARSession()
    {
        new GameObject("AR Session", typeof(ARSession), typeof(ARInputManager));
    }

    static XROrigin CreateXROrigin()
    {
        var originGo = new GameObject("XR Origin", typeof(XROrigin));
        var offset = new GameObject("Camera Offset");
        offset.transform.SetParent(originGo.transform, false);

        var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener),
            typeof(ARCameraManager), typeof(ARCameraBackground), typeof(TrackedPoseDriver));
        camGo.tag = "MainCamera";
        camGo.transform.SetParent(offset.transform, false);

        var cam = camGo.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = Color.black;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 20f;

        var driver = camGo.GetComponent<TrackedPoseDriver>();
        var pos = new InputAction("Position", binding: "<XRHMD>/centerEyePosition", expectedControlType: "Vector3");
        pos.AddBinding("<HandheldARInputDevice>/devicePosition");
        var rot = new InputAction("Rotation", binding: "<XRHMD>/centerEyeRotation", expectedControlType: "Quaternion");
        rot.AddBinding("<HandheldARInputDevice>/deviceRotation");
        driver.positionInput = new InputActionProperty(pos);
        driver.rotationInput = new InputActionProperty(rot);

        var origin = originGo.GetComponent<XROrigin>();
        origin.CameraFloorOffsetObject = offset;
        origin.Camera = cam;
        return origin;
    }

    // ---------- prefabs (placeholders until the Asset Store models arrive) ----------

    static GameObject CreatePlanePrefab()
    {
        string path = PrefabFolder + "/AR Plane.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        Selection.activeGameObject = null;
        EditorApplication.ExecuteMenuItem("GameObject/XR/AR Default Plane");
        GameObject plane = Selection.activeGameObject;
        var prefab = PrefabUtility.SaveAsPrefabAsset(plane, path);
        Object.DestroyImmediate(plane);
        return prefab;
    }

    static GameObject CreateIndicatorPrefab()
    {
        string path = PrefabFolder + "/Placement Indicator.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var root = new GameObject("Placement Indicator");
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Ring";
        ring.transform.SetParent(root.transform, false);
        ring.transform.localScale = new Vector3(0.35f, 0.002f, 0.35f);
        Object.DestroyImmediate(ring.GetComponent<Collider>());
        ring.GetComponent<MeshRenderer>().sharedMaterial = GetMaterial("Indicator", new Color(0.1f, 0.75f, 0.65f));

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static GameObject CreateBenchPrefab()
    {
        string path = PrefabFolder + "/Lab Bench.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var root = new GameObject("Lab Bench");
        var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "Board";
        board.transform.SetParent(root.transform, false);
        board.transform.localScale = new Vector3(0.6f, 0.03f, 0.4f);
        board.transform.localPosition = new Vector3(0f, 0.015f, 0f);
        board.GetComponent<MeshRenderer>().sharedMaterial = GetMaterial("Bench Wood", new Color(0.55f, 0.38f, 0.22f));

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static Material GetMaterial(string name, Color color)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;

        mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
