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

        // Placement manager + preview indicator.
        var managers = new GameObject("Managers");
        var placement = new GameObject("AR Placement Manager").AddComponent<ARPlacementManager>();
        placement.transform.SetParent(managers.transform);

        GameObject indicator = (GameObject)PrefabUtility.InstantiatePrefab(CreateIndicatorPrefab());
        indicator.name = "Placement Indicator";

        var so = new SerializedObject(placement);
        so.FindProperty("raycastManager").objectReferenceValue = raycastManager;
        so.FindProperty("planeManager").objectReferenceValue = planeManager;
        so.FindProperty("benchPrefab").objectReferenceValue = CreateBenchPrefab();
        so.FindProperty("previewIndicator").objectReferenceValue = indicator.transform;
        so.ApplyModifiedPropertiesWithoutUndo();

        CreateSetupUI(placement);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Debug.Log("[LabSceneBuilder] Lab scene created at " + ScenePath);
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

    // ---------- setup UI ----------

    static void CreateSetupUI(ARPlacementManager placement)
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var canvasGo = new GameObject("UI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        var panel = new GameObject("Setup Panel", typeof(RectTransform));
        panel.transform.SetParent(canvasGo.transform, false);
        Stretch(panel.GetComponent<RectTransform>());

        // Instruction banner at the top.
        var banner = new GameObject("Instruction Banner", typeof(Image));
        banner.transform.SetParent(panel.transform, false);
        banner.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        var br = banner.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0.05f, 1f); br.anchorMax = new Vector2(0.95f, 1f);
        br.pivot = new Vector2(0.5f, 1f);
        br.anchoredPosition = new Vector2(0f, -120f);
        br.sizeDelta = new Vector2(0f, 160f);

        var textGo = TMP_DefaultControls.CreateText(new TMP_DefaultControls.Resources());
        textGo.name = "Instruction Text";
        textGo.transform.SetParent(banner.transform, false);
        Stretch(textGo.GetComponent<RectTransform>());
        var text = textGo.GetComponent<TextMeshProUGUI>();
        text.text = "Move your phone slowly to scan the table";
        text.fontSize = 44;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        Button place = CreateButton(panel.transform, "Place Button", "Place bench", new Color(0.1f, 0.45f, 0.9f));
        Button move = CreateButton(panel.transform, "Reposition Button", "Move bench", new Color(0.3f, 0.3f, 0.3f));

        var ui = panel.AddComponent<PlacementUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("placementManager").objectReferenceValue = placement;
        so.FindProperty("instructionText").objectReferenceValue = text;
        so.FindProperty("placeButton").objectReferenceValue = place;
        so.FindProperty("repositionButton").objectReferenceValue = move;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static Button CreateButton(Transform parent, string name, string label, Color color)
    {
        var res = new TMP_DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd")
        };
        GameObject go = TMP_DefaultControls.CreateButton(res);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 160f);
        rt.sizeDelta = new Vector2(600f, 150f);

        var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 48;
        tmp.color = Color.white;
        return go.GetComponent<Button>();
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
