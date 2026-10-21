using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ARCircuitsLab.Circuit;
using ARCircuitsLab.Pooling;

/// <summary>
/// Builds simple placeholder models for the circuit parts, the bench slot,
/// the wire and the electron, saves them as prefabs, and creates the
/// ComponentData assets that hold each part's values.
/// Menu: AR Circuits Lab > Create Part Prefabs
/// </summary>
public static class PartPrefabBuilder
{
    const string PrefabFolder = "Assets/Prefabs/Parts";
    const string MaterialFolder = "Assets/Materials/Parts";
    const string DataFolder = "Assets/Data/Parts";

    [MenuItem("AR Circuits Lab/Create Part Prefabs")]
    public static void CreateAll()
    {
        Folder("Assets/Prefabs", "Parts");
        Folder("Assets/Materials", "Parts");
        Folder("Assets", "Data");
        Folder("Assets/Data", "Parts");

        var battery = Data("Battery", PartType.Battery, "Battery", 3f,
            "Battery: pushes current around the loop with 3 volts. The red end is +.");
        var sw = Data("Switch", PartType.Switch, "Switch", 0.01f,
            "Switch: opens or closes the loop. Tap it on the bench to turn it on or off.");
        var bulb = Data("Bulb", PartType.Bulb, "Bulb", 12f,
            "Bulb: lights up when current flows through it. More current makes it brighter.");
        var resistor = Data("Resistor", PartType.Resistor, "Resistor", 12f,
            "Resistor: slows the current down with 12 ohms of resistance.");

        BuildBattery(battery);
        BuildSwitch(sw);
        BuildBulb(bulb);
        BuildResistor(resistor);
        BuildSlot();
        BuildWire();
        BuildElectron();

        AssetDatabase.SaveAssets();
        Debug.Log("[PartPrefabBuilder] Part prefabs and data created.");
    }

    // ------------------------------------------------------------ parts

    static void BuildBattery(ComponentData data)
    {
        var root = new GameObject("Battery");
        Cube(root, "Body", new Vector3(0, 0.02f, 0), new Vector3(0.075f, 0.04f, 0.04f), Lit("Battery Body", Hex("3A2A12")));
        Cube(root, "Cap", new Vector3(0.03f, 0.02f, 0), new Vector3(0.016f, 0.041f, 0.041f), Lit("Battery Cap", Hex("FFC93C")));
        AddBox(root, new Vector3(0, 0.022f, 0), new Vector3(0.07f, 0.044f, 0.04f));
        Terminal a = MakeTerminal(root, "Terminal -", new Vector3(-0.045f, 0.03f, 0), Lit("Terminal Minus", Hex("2B2B2B")));
        Terminal b = MakeTerminal(root, "Terminal +", new Vector3(0.045f, 0.03f, 0), Lit("Terminal Plus", Hex("E5484D")));
        var c = root.AddComponent<Battery>();
        Setup(c, data, a, b);
        Save(root, "Battery");
    }

    static void BuildSwitch(ComponentData data)
    {
        var root = new GameObject("Switch");
        Cube(root, "Base", new Vector3(0, 0.006f, 0), new Vector3(0.08f, 0.012f, 0.04f), Lit("Switch Base", Hex("7A6440")));
        Cube(root, "Contact", new Vector3(0.026f, 0.015f, 0), new Vector3(0.008f, 0.008f, 0.014f), Metal());
        var pivot = new GameObject("Lever Pivot");
        pivot.transform.SetParent(root.transform, false);
        pivot.transform.localPosition = new Vector3(-0.026f, 0.018f, 0);
        Cube(pivot, "Lever", new Vector3(0.026f, 0, 0), new Vector3(0.054f, 0.006f, 0.012f), Metal());
        Cube(pivot, "Handle", new Vector3(0.05f, 0.006f, 0), new Vector3(0.008f, 0.014f, 0.02f), Lit("Switch Handle", Hex("FF8A3D")));
        AddBox(root, new Vector3(0, 0.018f, 0), new Vector3(0.07f, 0.036f, 0.04f));
        Terminal a = MakeTerminal(root, "Terminal A", new Vector3(-0.045f, 0.012f, 0), Metal());
        Terminal b = MakeTerminal(root, "Terminal B", new Vector3(0.045f, 0.012f, 0), Metal());
        var c = root.AddComponent<Switch>();
        Setup(c, data, a, b);
        Set(c, "lever", pivot.transform);
        Save(root, "Switch");
    }

    static void BuildBulb(ComponentData data)
    {
        var root = new GameObject("Bulb");
        Cube(root, "Base", new Vector3(0, 0.006f, 0), new Vector3(0.07f, 0.012f, 0.035f), Lit("Bulb Base", Hex("7A6440")));
        Shape(root, PrimitiveType.Cylinder, "Socket", new Vector3(0, 0.02f, 0), new Vector3(0.022f, 0.008f, 0.022f), Metal());
        Renderer glass = Shape(root, PrimitiveType.Sphere, "Glass", new Vector3(0, 0.046f, 0), Vector3.one * 0.045f, Glass());

        var lightGo = new GameObject("Glow");
        lightGo.transform.SetParent(root.transform, false);
        lightGo.transform.localPosition = new Vector3(0, 0.05f, 0);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 0.3f;
        light.color = Hex("FFD27A");
        light.intensity = 0f;
        light.shadows = LightShadows.None;
        light.enabled = false;

        AddBox(root, new Vector3(0, 0.035f, 0), new Vector3(0.07f, 0.07f, 0.04f));
        Terminal a = MakeTerminal(root, "Terminal A", new Vector3(-0.045f, 0.012f, 0), Metal());
        Terminal b = MakeTerminal(root, "Terminal B", new Vector3(0.045f, 0.012f, 0), Metal());
        var c = root.AddComponent<Bulb>();
        Setup(c, data, a, b);
        Set(c, "glass", glass);
        Set(c, "glow", light);
        Save(root, "Bulb");
    }

    static void BuildResistor(ComponentData data)
    {
        var root = new GameObject("Resistor");
        Cube(root, "Leads", new Vector3(0, 0.018f, 0), new Vector3(0.088f, 0.003f, 0.003f), Metal());
        Renderer body = Shape(root, PrimitiveType.Cylinder, "Body", new Vector3(0, 0.018f, 0), new Vector3(0.02f, 0.026f, 0.02f), Lit("Resistor Body", Hex("E8D3A8")));
        body.transform.localRotation = Quaternion.Euler(0, 0, 90);
        string[] bands = { "8B4A1F", "3A2A12", "E5484D" };
        for (int i = 0; i < bands.Length; i++)
        {
            Renderer band = Shape(root, PrimitiveType.Cylinder, "Band " + (i + 1), new Vector3(-0.012f + 0.012f * i, 0.018f, 0),
                new Vector3(0.0215f, 0.0018f, 0.0215f), Lit("Band " + bands[i], Hex(bands[i])));
            band.transform.localRotation = Quaternion.Euler(0, 0, 90);
        }
        AddBox(root, new Vector3(0, 0.018f, 0), new Vector3(0.06f, 0.036f, 0.03f));
        Terminal a = MakeTerminal(root, "Terminal A", new Vector3(-0.045f, 0.018f, 0), Metal());
        Terminal b = MakeTerminal(root, "Terminal B", new Vector3(0.045f, 0.018f, 0), Metal());
        var c = root.AddComponent<Resistor>();
        Setup(c, data, a, b);
        Save(root, "Resistor");
    }

    // ------------------------------------------------------------ bench pieces

    static void BuildSlot()
    {
        var root = new GameObject("Bench Slot");
        Renderer pad = Cube(root, "Pad", new Vector3(0, 0.0015f, 0), new Vector3(0.12f, 0.003f, 0.13f), Lit("Slot Pad", Hex("B98552")));
        AddBox(root, new Vector3(0, 0.002f, 0), new Vector3(0.12f, 0.004f, 0.13f));
        var slot = root.AddComponent<BenchSlot>();
        Set(slot, "visual", pad);
        Save(root, "Bench Slot");
    }

    static void BuildWire()
    {
        var root = new GameObject("Wire");
        var line = root.AddComponent<LineRenderer>();
        line.sharedMaterial = Unlit("Wire", Hex("D92D2D"));
        line.widthMultiplier = 0.005f;
        line.numCapVertices = 3;
        line.numCornerVertices = 2;
        line.useWorldSpace = false;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        root.AddComponent<Wire>();
        Save(root, "Wire");
    }

    static void BuildElectron()
    {
        var root = new GameObject("Electron");
        var dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object.DestroyImmediate(dot.GetComponent<Collider>());
        dot.name = "Dot";
        dot.transform.SetParent(root.transform, false);
        dot.transform.localScale = Vector3.one * 0.009f;
        var r = dot.GetComponent<MeshRenderer>();
        r.sharedMaterial = Unlit("Electron", Hex("FFE27A"));
        r.shadowCastingMode = ShadowCastingMode.Off;
        root.AddComponent<ElectronParticle>();
        Save(root, "Electron");
    }

    // ------------------------------------------------------------ helpers

    static Terminal MakeTerminal(GameObject parent, string name, Vector3 pos, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = Vector3.one * 0.016f;
        go.GetComponent<SphereCollider>().radius = 1.15f;   // bigger than it looks, so it is easy to tap
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        var t = go.AddComponent<Terminal>();
        Set(t, "visual", r);
        return t;
    }

    static void Setup(CircuitComponent c, ComponentData data, Terminal a, Terminal b)
    {
        Set(c, "data", data);
        Set(c, "terminalA", a);
        Set(c, "terminalB", b);
    }

    static Renderer Cube(GameObject parent, string name, Vector3 pos, Vector3 scale, Material mat) =>
        Shape(parent, PrimitiveType.Cube, name, pos, scale, mat);

    static Renderer Shape(GameObject parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        return r;
    }

    static void AddBox(GameObject root, Vector3 center, Vector3 size)
    {
        var box = root.AddComponent<BoxCollider>();
        box.center = center;
        box.size = size;
    }

    static void Set(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void Save(GameObject root, string name)
    {
        PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{name}.prefab");
        Object.DestroyImmediate(root);
    }

    static ComponentData Data(string file, PartType type, string displayName, float value, string description)
    {
        string path = $"{DataFolder}/{file}.asset";
        var d = AssetDatabase.LoadAssetAtPath<ComponentData>(path);
        if (d == null)
        {
            d = ScriptableObject.CreateInstance<ComponentData>();
            AssetDatabase.CreateAsset(d, path);
        }
        d.partType = type;
        d.displayName = displayName;
        d.description = description;
        d.value = value;
        d.internalResistance = 0.2f;
        d.ratedPower = 0.75f;   // 3 V across 12 Ω = 0.75 W, full brightness
        EditorUtility.SetDirty(d);
        return d;
    }

    static Material Metal() => Lit("Metal", Hex("C9CED6"), 0.8f);

    static Material Glass()
    {
        Material m = Lit("Bulb Glass", Hex("EBEBE0"), 0.6f);
        m.EnableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", Color.black);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        EditorUtility.SetDirty(m);
        return m;
    }

    static Material Lit(string name, Color color, float smoothness = 0.3f) =>
        GetMaterial(name, "Universal Render Pipeline/Lit", color, smoothness);

    static Material Unlit(string name, Color color) =>
        GetMaterial(name, "Universal Render Pipeline/Unlit", color, 0f);

    static Material GetMaterial(string name, string shader, Color color, float smoothness)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find(shader));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color c);
        return c;
    }

    static void Folder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{name}")) AssetDatabase.CreateFolder(parent, name);
    }
}
