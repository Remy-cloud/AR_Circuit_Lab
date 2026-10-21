using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small helpers that build uGUI elements in the "bright classroom" style.
/// Reference resolution is 1080 x 1920 (portrait).
/// </summary>
public static class UIKit
{
    // ---- Colours ----
    public static readonly Color Bg = Hex("FFF8EC");
    public static readonly Color Card = Color.white;
    public static readonly Color Border = Hex("F0D9B5");
    public static readonly Color Soft = Hex("F7E9D2");
    public static readonly Color Ink = Hex("3A2A12");
    public static readonly Color Muted = Hex("7A6440");
    public static readonly Color Orange = Hex("FF8A3D");
    public static readonly Color OrangeEdge = Hex("D9661F");
    public static readonly Color OnOrange = Hex("2E1500");
    public static readonly Color Sun = Hex("FFC93C");
    public static readonly Color SunTint = Hex("FFF1CF");
    public static readonly Color SunInk = Hex("5A3E00");
    public static readonly Color Mint = Hex("3CC47C");
    public static readonly Color MintTint = Hex("E3F7EC");
    public static readonly Color MintInk = Hex("14663D");
    public static readonly Color Red = Hex("E5484D");
    public static readonly Color RedTint = Hex("FDE7E7");
    public static readonly Color RedInk = Hex("B42318");

    public enum Style { Primary, Secondary, Danger }

    static Sprite rounded, roundedSmall, circle;

    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color c);
        return c;
    }

    // ---- Sprites ----
    public static Sprite Rounded => rounded ??= LoadSprite("Assets/UI/Sprites/rounded_24.png", 24);
    public static Sprite RoundedSmall => roundedSmall ??= LoadSprite("Assets/UI/Sprites/rounded_12.png", 12);
    public static Sprite Circle => circle ??= LoadSprite("Assets/UI/Sprites/circle.png", 0);

    static Sprite LoadSprite(string path, int border)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer.textureType != TextureImporterType.Sprite || importer.spriteBorder.x != border)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ---- Layout ----
    public static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    /// <summary>Pins a rect to the top, bottom or full width of its parent.</summary>
    public static void Dock(RectTransform rt, bool top, float height, float inset = 0f, float edge = 0f)
    {
        rt.anchorMin = new Vector2(0, top ? 1 : 0);
        rt.anchorMax = new Vector2(1, top ? 1 : 0);
        rt.pivot = new Vector2(0.5f, top ? 1 : 0);
        rt.sizeDelta = new Vector2(-inset * 2, height);
        rt.anchoredPosition = new Vector2(0, top ? -edge : edge);
    }

    public static VerticalLayoutGroup VStack(GameObject go, float spacing, RectOffset padding, TextAnchor align = TextAnchor.UpperLeft)
    {
        var v = go.AddComponent<VerticalLayoutGroup>();
        v.spacing = spacing; v.padding = padding; v.childAlignment = align;
        v.childControlWidth = true; v.childControlHeight = true;
        v.childForceExpandWidth = true; v.childForceExpandHeight = false;
        return v;
    }

    public static HorizontalLayoutGroup HStack(GameObject go, float spacing, RectOffset padding, TextAnchor align = TextAnchor.MiddleLeft)
    {
        var h = go.AddComponent<HorizontalLayoutGroup>();
        h.spacing = spacing; h.padding = padding; h.childAlignment = align;
        h.childControlWidth = true; h.childControlHeight = true;
        h.childForceExpandWidth = false; h.childForceExpandHeight = false;
        return h;
    }

    public static LayoutElement Size(Component c, float height = -1, float width = -1, float flexW = -1, float flexH = -1)
    {
        var le = c.gameObject.GetComponent<LayoutElement>() ?? c.gameObject.AddComponent<LayoutElement>();
        if (height >= 0) { le.preferredHeight = height; le.minHeight = height; }
        if (width >= 0) { le.preferredWidth = width; le.minWidth = width; }
        if (flexW >= 0) le.flexibleWidth = flexW;
        if (flexH >= 0) le.flexibleHeight = flexH;
        return le;
    }

    public static RectTransform Spacer(Transform parent) { var r = Rect("Spacer", parent); Size(r, flexH: 1); return r; }
    public static RectOffset Pad(int all) => new RectOffset(all, all, all, all);
    public static RectOffset Pad(int h, int v) => new RectOffset(h, h, v, v);
    public static RectOffset Pad(int l, int r, int t, int b) => new RectOffset(l, r, t, b);

    // ---- Elements ----
    public static Image Box(Transform parent, string name, Color color, Sprite sprite = null, float corner = 2f)
    {
        var rt = Rect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite ?? Rounded;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1f / corner;
        img.color = color;
        return img;
    }

    /// <summary>A white card with a soft beige outline.</summary>
    public static Image CardBox(Transform parent, string name, Color? fill = null, Color? outline = null)
    {
        var img = Box(parent, name, fill ?? Card);
        var o = img.gameObject.AddComponent<Outline>();
        o.effectColor = outline ?? Border;
        o.effectDistance = new Vector2(4, -4);
        return img;
    }

    public static TextMeshProUGUI Label(Transform parent, string text, float size, Color color, bool bold = false,
        TextAlignmentOptions align = TextAlignmentOptions.Left, string name = "Text")
    {
        var rt = Rect(name, parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = color;
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        t.alignment = align;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.raycastTarget = false;
        return t;
    }

    public static Button Button(Transform parent, string name, string label, Style style, float height = 160f, float fontSize = 50f)
    {
        Color fill = style switch { Style.Primary => Orange, Style.Danger => Red, _ => Card };
        Color text = style switch { Style.Primary => OnOrange, Style.Danger => Color.white, _ => Ink };

        var img = Box(parent, name, fill);
        if (style == Style.Secondary)
        {
            var o = img.gameObject.AddComponent<Outline>();
            o.effectColor = Border; o.effectDistance = new Vector2(4, -4);
        }
        else
        {
            // The darker "edge" under the button makes it look pressable.
            var s = img.gameObject.AddComponent<Shadow>();
            s.effectColor = style == Style.Primary ? OrangeEdge : RedInk;
            s.effectDistance = new Vector2(0, -10);
        }
        var b = img.gameObject.AddComponent<UnityEngine.UI.Button>();
        b.targetGraphic = img;
        var colors = b.colors; colors.disabledColor = new Color(1, 1, 1, 0.45f); b.colors = colors;
        Size(img, height);

        var t = Label(img.transform, label, fontSize, text, true, TextAlignmentOptions.Center, "Label");
        Stretch(t.rectTransform);
        return b;
    }

    /// <summary>A round icon-style button with a short text glyph (e.g. "×", "II").</summary>
    public static Button RoundButton(Transform parent, string name, string glyph, float size = 130f, Color? fill = null)
    {
        var img = Box(parent, name, fill ?? Card, Circle, 1f);
        img.type = Image.Type.Simple;
        var o = img.gameObject.AddComponent<Outline>();
        o.effectColor = Border; o.effectDistance = new Vector2(4, -4);
        var b = img.gameObject.AddComponent<UnityEngine.UI.Button>();
        b.targetGraphic = img;
        Size(img, size, size);
        var t = Label(img.transform, glyph, size * 0.42f, Ink, true, TextAlignmentOptions.Center, "Glyph");
        Stretch(t.rectTransform);
        return b;
    }

    public static Image Dot(Transform parent, string name, Color color, float size)
    {
        var img = Box(parent, name, color, Circle, 1f);
        img.type = Image.Type.Simple;
        Size(img, size, size);
        return img;
    }

    /// <summary>A coloured circle with centred text (numbers, badges).</summary>
    public static TextMeshProUGUI Badge(Transform parent, string name, string text, Color fill, Color ink, float size)
    {
        var dot = Dot(parent, name, fill, size);
        var t = Label(dot.transform, text, size * 0.45f, ink, true, TextAlignmentOptions.Center);
        Stretch(t.rectTransform);
        return t;
    }

    /// <summary>How far backgrounds reach past the safe area, so they fill the notch and home-bar zones.</summary>
    public const float Bleed = 400f;

    /// <summary>A background that fills the whole phone screen, even outside the safe area.</summary>
    public static Image FullBleed(Transform parent, string name, Color color)
    {
        RectTransform rt = Rect(name, parent);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-Bleed, -Bleed); rt.offsetMax = new Vector2(Bleed, Bleed);
        rt.SetAsFirstSibling();
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        return img;
    }

    /// <summary>A white sheet that slides up from the bottom (rounded top only).</summary>
    public static Image BottomSheet(Transform parent, string name, float height)
    {
        var img = Box(parent, name, Card);
        Dock(img.rectTransform, false, height + Bleed, 0f, -Bleed); // runs past the home bar to the screen edge
        return img;
    }

    public static void Wire(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
