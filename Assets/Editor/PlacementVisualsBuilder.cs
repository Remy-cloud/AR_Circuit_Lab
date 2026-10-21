using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Custom look for AR placement:
///   - detected table surfaces show a soft grid with "Intwali Remy" on it
///   - the spot where the bench will go shows a bench-sized marker with the name
/// Menu: AR Circuits Lab > Create Placement Visuals
/// </summary>
public static class PlacementVisualsBuilder
{
    const string TextureFolder = "Assets/Textures/Placement";
    const string MaterialFolder = "Assets/Materials";
    const string PlanePrefab = "Assets/Prefabs/AR Plane.prefab";
    const string IndicatorPrefab = "Assets/Prefabs/Placement Indicator.prefab";

    // Colours are drawn into the textures (light blue grid, white name), so the tint is plain white.
    static readonly Color LightBlue = new Color(0.49f, 0.78f, 1f);

    // The bench is 60 cm x 40 cm at scale 0.6, so 36 cm x 24 cm on the table.
    static readonly Vector2 BenchSize = new Vector2(0.36f, 0.24f);

    [MenuItem("AR Circuits Lab/Create Placement Visuals")]
    public static GameObject BuildAll()
    {
        Texture2D grid = ImportTexture("PlaneGrid.png", TextureWrapMode.Repeat);
        Texture2D marker = ImportTexture("BenchMarker.png", TextureWrapMode.Clamp);

        // One texture tile covers 0.5 m of table (plane UVs are in metres).
        Material gridMat = Transparent("Plane Grid", grid, new Color(1f, 1f, 1f, 0.85f), Vector2.one * 2f);
        Material markerMat = Transparent("Bench Marker", marker, Color.white, Vector2.one);

        StylePlane(gridMat);
        GameObject indicator = BuildIndicator(markerMat);
        AssetDatabase.SaveAssets();
        Debug.Log("[PlacementVisualsBuilder] Surface grid and bench marker updated.");
        return indicator;
    }

    static void StylePlane(Material gridMat)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PlanePrefab) == null) return;
        GameObject root = PrefabUtility.LoadPrefabContents(PlanePrefab);

        foreach (MeshRenderer r in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            r.sharedMaterial = gridMat;
            r.shadowCastingMode = ShadowCastingMode.Off;
        }
        foreach (LineRenderer line in root.GetComponentsInChildren<LineRenderer>(true))
        {
            line.sharedMaterial = Unlit("Plane Edge", LightBlue);
            line.widthMultiplier = 1f;
            line.startWidth = line.endWidth = 0.004f;
            line.startColor = line.endColor = Color.white;
        }

        PrefabUtility.SaveAsPrefabAsset(root, PlanePrefab);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static GameObject BuildIndicator(Material markerMat)
    {
        var root = new GameObject("Placement Indicator");
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Object.DestroyImmediate(quad.GetComponent<Collider>());
        quad.name = "Marker";
        quad.transform.SetParent(root.transform, false);
        quad.transform.localPosition = new Vector3(0f, 0.002f, 0f);
        // Lie flat, with the text the right way up for the student (the indicator faces the camera).
        quad.transform.localRotation = Quaternion.Euler(90f, 180f, 0f);
        quad.transform.localScale = new Vector3(BenchSize.x, BenchSize.y, 1f);
        var r = quad.GetComponent<MeshRenderer>();
        r.sharedMaterial = markerMat;
        r.shadowCastingMode = ShadowCastingMode.Off;
        r.receiveShadows = false;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, IndicatorPrefab);
        Object.DestroyImmediate(root);
        return prefab;
    }

    // ------------------------------------------------------------ helpers

    static Texture2D ImportTexture(string file, TextureWrapMode wrap)
    {
        string path = $"{TextureFolder}/{file}";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Default;
        importer.alphaIsTransparency = true;
        importer.wrapMode = wrap;
        importer.mipmapEnabled = true;
        importer.sRGBTexture = true;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static Material Transparent(string name, Texture2D tex, Color tint, Vector2 tiling)
    {
        Material m = GetMaterial(name, "Universal Render Pipeline/Unlit");
        m.SetTexture("_BaseMap", tex);
        m.SetTextureScale("_BaseMap", tiling);
        m.SetColor("_BaseColor", tint);
        m.SetFloat("_Surface", 1f);   // transparent
        m.SetFloat("_Blend", 0f);     // alpha blend
        m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
        m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.SetFloat("_Cull", (float)CullMode.Off);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = (int)RenderQueue.Transparent;
        EditorUtility.SetDirty(m);
        return m;
    }

    static Material Unlit(string name, Color color)
    {
        Material m = GetMaterial(name, "Universal Render Pipeline/Unlit");
        m.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(m);
        return m;
    }

    static Material GetMaterial(string name, string shader)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find(shader));
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }
}
