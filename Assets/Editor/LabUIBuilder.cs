using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using ARCircuitsLab.AR;
using ARCircuitsLab.States;
using ARCircuitsLab.UI;
using static UIKit;

/// <summary>
/// Builds every screen of the app (bright classroom style) on one canvas.
/// Each screen is a panel; the UI Manager shows the right ones per state.
/// </summary>
public static class LabUIBuilder
{
    public struct Screens
    {
        public GameObject home, briefing, setup, hud, pause, summary, history;
    }

    public static Screens Build(AppStateManager app, ARPlacementManager placement)
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var canvasGo = new GameObject("UI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0f;
        Transform root = canvasGo.transform;

        // The safe area keeps content away from the iPhone notch and home bar.
        var safe = Rect("Safe Area", root); Stretch(safe);
        safe.gameObject.AddComponent<SafeArea>();

        return new Screens
        {
            home = BuildHome(safe, app),
            briefing = BuildBriefing(safe, app),
            setup = BuildSetup(safe, app, placement),
            hud = BuildHud(safe, app),
            pause = BuildPause(safe, app),
            summary = BuildSummary(safe, app),
            history = BuildHistory(safe, app),
        };
    }

    // ------------------------------------------------------------------ helpers

    static GameObject FullScreen(Transform parent, string name, bool background)
    {
        RectTransform rt = Rect(name, parent); Stretch(rt);
        if (background)
        {
            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = Bg;
        }
        return rt.gameObject;
    }

    static RectTransform Column(GameObject screen, RectOffset pad, float spacing)
    {
        RectTransform col = Rect("Content", screen.transform); Stretch(col);
        VStack(col.gameObject, spacing, pad);
        return col;
    }

    static RectTransform Row(Transform parent, string name, float spacing, TextAnchor align = TextAnchor.MiddleLeft)
    {
        RectTransform r = Rect(name, parent);
        HStack(r.gameObject, spacing, Pad(0), align);
        return r;
    }

    static T Add<T>(GameObject go, AppStateManager app) where T : ViewBase
    {
        var v = go.AddComponent<T>();
        Wire(v, "app", app);
        return v;
    }

    static void Set(Object target, string field, Object value) => Wire(target, field, value);

    static TextMeshProUGUI Title(Transform p, string text, float size = 84f) => Label(p, text, size, Ink, true, name: "Title");
    static TextMeshProUGUI Body(Transform p, string text, float size = 42f) => Label(p, text, size, Muted, name: "Body");

    // ------------------------------------------------------------------ Home

    static GameObject BuildHome(Transform parent, AppStateManager app)
    {
        GameObject s = FullScreen(parent, "Home Screen", true);
        RectTransform col = Column(s, Pad(56, 56, 50, 60), 34);

        RectTransform header = Row(col, "Header", 28);
        Badge(header, "Logo", "", Sun, SunInk, 110);
        Label(header, "AR Circuits Lab", 56, Ink, true);

        Image hero = CardBox(col, "Hero Card");
        VStack(hero.gameObject, 26, Pad(48, 44));
        Image art = Box(hero.transform, "Illustration", SunTint);
        Size(art, 240);
        HStack(art.gameObject, 40, Pad(40), TextAnchor.MiddleCenter);
        Image bat = Box(art.transform, "Battery", Ink, RoundedSmall, 1f); Size(bat, 110, 90);
        Image wire = Box(art.transform, "Wire", Orange, RoundedSmall, 1f); Size(wire, 16, 300);
        Dot(art.transform, "Bulb", Sun, 150);
        Title(hero.transform, "Light up your first circuit", 70);
        Body(hero.transform, "Build real circuits on your desk with your phone's camera. No kit needed.");
        Button start = Button(hero.transform, "Start Button", "Start lab", Style.Primary);

        Label(col, "Your experiments", 52, Ink, true);
        ExperimentRow(col, "1", "Light the bulb", "Make your first closed loop", "Start here", true);
        ExperimentRow(col, "2", "Series or parallel", "Make two bulbs shine brightly", "Locked", false);
        ExperimentRow(col, "3", "Fix the broken circuit", "Find the faulty part", "Locked", false);

        Spacer(col);
        Image bestRow = Box(col, "Best Score", MintTint);
        var best = bestRow.gameObject.AddComponent<UnityEngine.UI.Button>(); best.targetGraphic = bestRow;
        HStack(bestRow.gameObject, 28, Pad(40, 32), TextAnchor.MiddleLeft);
        Badge(bestRow.transform, "Trophy", "1st", Mint, Color.white, 100);
        RectTransform bestTexts = Rect("Texts", bestRow.transform); VStack(bestTexts.gameObject, 4, Pad(0)); Size(bestTexts, flexW: 1);
        Label(bestTexts, "Your best score", 36, MintInk);
        TextMeshProUGUI bestScore = Label(bestTexts, "No labs yet", 50, MintInk, true, name: "Best Score Text");
        Label(bestRow.transform, "History", 42, MintInk, true);

        var v = Add<HomeView>(s, app);
        Set(v, "startButton", start); Set(v, "historyButton", best); Set(v, "bestScoreText", bestScore);
        return s;
    }

    static void ExperimentRow(Transform parent, string number, string title, string sub, string tag, bool active)
    {
        Image row = CardBox(parent, "Experiment " + number, Card, active ? Sun : Border);
        HStack(row.gameObject, 28, Pad(34, 24), TextAnchor.MiddleLeft);
        Badge(row.transform, "Number", number, active ? Sun : Soft, active ? SunInk : Muted, 100);
        RectTransform texts = Rect("Texts", row.transform); VStack(texts.gameObject, 4, Pad(0)); Size(texts, flexW: 1);
        Label(texts, title, 44, Ink, true);
        Label(texts, sub, 36, Muted);
        Image chip = Box(row.transform, "Tag", active ? SunTint : Soft, RoundedSmall, 0.5f);
        HStack(chip.gameObject, 0, Pad(26, 12), TextAnchor.MiddleCenter);
        Label(chip.transform, tag, 32, active ? SunInk : Muted, true).textWrappingMode = TextWrappingModes.NoWrap;
    }

    // ------------------------------------------------------------------ Briefing

    static GameObject BuildBriefing(Transform parent, AppStateManager app)
    {
        GameObject s = FullScreen(parent, "Briefing Screen", true);
        RectTransform col = Column(s, Pad(56, 56, 60, 70), 44);

        RectTransform header = Row(col, "Header", 28);
        Button back = RoundButton(header, "Back Button", "‹");
        Label(header, "Experiment 1 of 3", 40, Muted);

        Image art = CardBox(col, "Illustration");
        Size(art, 500);
        HStack(art.gameObject, 50, Pad(60), TextAnchor.MiddleCenter);
        Image bat = Box(art.transform, "Battery", Ink, RoundedSmall, 1f); Size(bat, 130, 100);
        Image sw = Box(art.transform, "Switch", Muted, RoundedSmall, 1f); Size(sw, 60, 180);
        Image bulb = Dot(art.transform, "Bulb", Card, 150);
        var bo = bulb.gameObject.AddComponent<Outline>(); bo.effectColor = Ink; bo.effectDistance = new Vector2(6, -6);

        Title(col, "Light the bulb", 88);
        Label(col, "Connect a battery, a switch and a bulb in one loop. Then close the switch.", 46, Ink);

        Label(col, "You'll use", 38, Muted, true);
        RectTransform chips = Row(col, "Parts", 20);
        foreach (string p in new[] { "1 battery", "1 switch", "1 bulb", "3 wires" })
        {
            Image chip = CardBox(chips, p);
            HStack(chip.gameObject, 0, Pad(30, 18), TextAnchor.MiddleCenter);
            Label(chip.transform, p, 38, Ink, true).textWrappingMode = TextWrappingModes.NoWrap;
        }

        Image learn = Box(col, "What you'll learn", SunTint);
        VStack(learn.gameObject, 8, Pad(44, 36));
        Label(learn.transform, "What you'll learn", 42, SunInk, true);
        Label(learn.transform, "Current only flows when the path is a complete loop.", 40, SunInk);

        Spacer(col);
        Label(col, "You'll need a table and good light.", 36, Muted, align: TextAlignmentOptions.Center);
        Button find = Button(col, "Find Table Button", "Find a table", Style.Primary);

        var v = Add<BriefingView>(s, app);
        Set(v, "backButton", back); Set(v, "findTableButton", find);
        return s;
    }

    // ------------------------------------------------------------------ Setup (camera)

    static GameObject BuildSetup(Transform parent, AppStateManager app, ARPlacementManager placement)
    {
        GameObject s = FullScreen(parent, "Setup Screen", false);

        RectTransform top = Rect("Top Bar", s.transform);
        Dock(top, true, 130, 48, 60);
        HStack(top.gameObject, 24, Pad(0), TextAnchor.MiddleLeft);
        Button close = RoundButton(top, "Close Button", "×");
        Image stepper = Box(top, "Stepper", Card); Size(stepper, 130, flexW: 1);
        HStack(stepper.gameObject, 30, Pad(40, 26), TextAnchor.MiddleCenter);
        Image scanStep = Step(stepper.transform, "Scan");
        Image placeStep = Step(stepper.transform, "Place");

        Image sheet = BottomSheet(s.transform, "Bottom Sheet", 560);
        VStack(sheet.gameObject, 30, Pad(56, 56, 34, 120), TextAnchor.UpperCenter);
        Handle(sheet.transform);
        TextMeshProUGUI title = Label(sheet.transform, "Looking for a table", 62, Ink, true, name: "Title");
        TextMeshProUGUI body = Label(sheet.transform, "Move your phone slowly over a flat surface.", 42, Muted, name: "Body");
        Button place = Button(sheet.transform, "Place Button", "Place lab here", Style.Primary);

        var v = Add<PlacementUI>(s, app);
        Set(v, "placementManager", placement); Set(v, "titleText", title); Set(v, "bodyText", body);
        Set(v, "placeButton", place); Set(v, "closeButton", close);
        Set(v, "scanStep", scanStep); Set(v, "placeStep", placeStep);
        return s;
    }

    static void Handle(Transform sheet)
    {
        Image h = Box(sheet, "Handle", Border, RoundedSmall, 1f);
        var le = h.gameObject.AddComponent<LayoutElement>(); le.ignoreLayout = true;
        RectTransform r = h.rectTransform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 1f);
        r.sizeDelta = new Vector2(110, 14); r.anchoredPosition = new Vector2(0, -24);
    }

    static Image Step(Transform parent, string label)
    {
        RectTransform col = Rect(label + " Step", parent); VStack(col.gameObject, 10, Pad(0)); Size(col, flexW: 1);
        Label(col, label, 38, Ink, true);
        Image bar = Box(col, "Bar", Orange, RoundedSmall, 2f); Size(bar, 12);
        return bar;
    }

    // ------------------------------------------------------------------ HUD (camera)

    static GameObject BuildHud(Transform parent, AppStateManager app)
    {
        GameObject s = FullScreen(parent, "Lab HUD", false);
        var v = Add<HudView>(s, app);

        // Goal card + pause
        RectTransform top = Rect("Top Bar", s.transform);
        Dock(top, true, 300, 44, 56);
        HStack(top.gameObject, 24, Pad(0), TextAnchor.UpperLeft);
        Image goal = CardBox(top, "Goal Card"); Size(goal, flexW: 1);
        VStack(goal.gameObject, 8, Pad(40, 32));
        TextMeshProUGUI exp = Label(goal.transform, "Experiment 1 of 3", 34, Muted, name: "Experiment");
        TextMeshProUGUI title = Label(goal.transform, "Light the bulb", 54, Ink, true, name: "Title");
        TextMeshProUGUI goalText = Label(goal.transform, "Connect a battery, a switch and a bulb.", 38, Ink, name: "Goal");
        Button pause = RoundButton(top, "Pause Button", "II");

        // Readings
        Image readings = CardBox(s.transform, "Readings");
        RectTransform rr = readings.rectTransform;
        rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(1, 0);
        rr.anchoredPosition = new Vector2(-44, 760); rr.sizeDelta = new Vector2(520, 150);
        HStack(readings.gameObject, 40, Pad(36, 22), TextAnchor.MiddleLeft);
        TextMeshProUGUI current = Reading(readings.transform, "Current", "0.00 A");
        TextMeshProUGUI circuit = Reading(readings.transform, "Circuit", "Open");
        TextMeshProUGUI score = Reading(readings.transform, "Score", "0");

        // Hint bubble
        Image hint = CardBox(s.transform, "Hint Bubble");
        RectTransform hr = hint.rectTransform;
        hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0.5f); hr.pivot = new Vector2(0.5f, 0.5f);
        hr.anchoredPosition = new Vector2(0, 260); hr.sizeDelta = new Vector2(760, 170);
        TextMeshProUGUI hintText = Label(hint.transform, "Tap a part below, then tap the bench.", 40, Ink, name: "Hint");
        Stretch(hintText.rectTransform); hintText.margin = new Vector4(36, 20, 36, 20);
        hintText.alignment = TextAlignmentOptions.Center;

        // Parts drawer
        Image drawer = BottomSheet(s.transform, "Parts Drawer", 560);
        VStack(drawer.gameObject, 28, Pad(44, 44, 34, 110), TextAnchor.UpperCenter);
        Handle(drawer.transform);
        RectTransform dh = Row(drawer.transform, "Header", 20);
        TextMeshProUGUI partsLabel = Label(dh, "Parts", 48, Ink, true); Size(partsLabel, flexW: 1);
        Button undo = Button(dh, "Undo Button", "Undo", Style.Secondary, 100, 38); Size(undo, 100, 220);
        RectTransform grid = Rect("Parts Row", drawer.transform); Size(grid, 230);
        var gl = HStack(grid.gameObject, 20, Pad(0), TextAnchor.MiddleCenter);
        gl.childForceExpandWidth = true; gl.childForceExpandHeight = true;
        Button battery = Part(grid, "Battery"); Button sw = Part(grid, "Switch");
        Button bulb = Part(grid, "Bulb"); Button resistor = Part(grid, "Resistor");

        // Success sheet (hidden until an experiment is finished)
        Image success = BottomSheet(s.transform, "Success Sheet", 900);
        VStack(success.gameObject, 30, Pad(56, 56, 60, 120));
        success.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        RectTransform sh = Row(success.transform, "Header", 30);
        TextMeshProUGUI pts = Badge(sh, "Points", "+100", Mint, Color.white, 150);
        pts.fontSize = 44; pts.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform st = Rect("Texts", sh); VStack(st.gameObject, 4, Pad(0)); Size(st, flexW: 1);
        TextMeshProUGUI sTitle = Label(st, "The bulb is on!", 66, Ink, true, name: "Title");
        Label(st, "Experiment complete", 38, Muted);
        Image why = Box(success.transform, "Why Box", SunTint);
        VStack(why.gameObject, 0, Pad(44, 36));
        TextMeshProUGUI sBody = Label(why.transform, "Closing the switch completed the loop, so current can flow.", 42, SunInk, name: "Body");
        Button cont = Button(success.transform, "Continue Button", "Next experiment", Style.Primary);

        // Short-circuit warning (hidden until it happens)
        Image warn = CardBox(s.transform, "Warning Banner", Card, Red);
        Dock(warn.rectTransform, true, 420, 44, 56);
        VStack(warn.gameObject, 22, Pad(44, 36));
        warn.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        RectTransform wh = Row(warn.transform, "Header", 24);
        Badge(wh, "Alert", "!", RedTint, RedInk, 100);
        Label(wh, "Short circuit!", 60, RedInk, true);
        Label(warn.transform, "The battery's two ends are joined with nothing in between. A real battery would get hot.", 40, Ink);
        Button wUndo = Button(warn.transform, "Undo Wire Button", "Undo wire", Style.Danger, 130, 44);

        Set(v, "experimentText", exp); Set(v, "titleText", title); Set(v, "goalText", goalText);
        Set(v, "pauseButton", pause); Set(v, "currentText", current); Set(v, "circuitText", circuit);
        Set(v, "scoreText", score); Set(v, "hintBubble", hint.gameObject); Set(v, "hintText", hintText);
        Set(v, "batteryButton", battery); Set(v, "switchButton", sw); Set(v, "bulbButton", bulb);
        Set(v, "resistorButton", resistor); Set(v, "undoButton", undo);
        Set(v, "successSheet", success.gameObject); Set(v, "successTitle", sTitle); Set(v, "successBody", sBody);
        Set(v, "continueButton", cont); Set(v, "warningBanner", warn.gameObject); Set(v, "warningUndoButton", wUndo);
        return s;
    }

    static TextMeshProUGUI Reading(Transform parent, string label, string value)
    {
        RectTransform c = Rect(label, parent); VStack(c.gameObject, 2, Pad(0));
        Label(c, label, 30, Muted);
        return Label(c, value, 44, Ink, true, name: "Value");
    }

    static Button Part(Transform parent, string name)
    {
        Image img = CardBox(parent, name + " Part", Bg);
        var b = img.gameObject.AddComponent<UnityEngine.UI.Button>(); b.targetGraphic = img;
        var pv = VStack(img.gameObject, 14, Pad(10, 30), TextAnchor.MiddleCenter);
        pv.childForceExpandWidth = false;
        Size(img, 230, 200, flexW: 1);
        Color c = name == "Battery" ? Ink : name == "Bulb" ? Sun : name == "Switch" ? Muted : Orange;
        Image icon = Box(img.transform, "Icon", c, name == "Bulb" ? Circle : RoundedSmall, 1f);
        if (name == "Bulb") icon.type = Image.Type.Simple;
        Size(icon, 80, name == "Switch" || name == "Resistor" ? 120 : 80);
        Label(img.transform, name, 36, Ink, true, TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.NoWrap;
        return b;
    }

    // ------------------------------------------------------------------ Pause

    static GameObject BuildPause(Transform parent, AppStateManager app)
    {
        GameObject s = FullScreen(parent, "Pause Screen", false);
        var dim = s.AddComponent<Image>(); dim.color = new Color(0.1f, 0.07f, 0.03f, 0.55f);

        Image card = CardBox(s.transform, "Pause Card");
        RectTransform cr = card.rectTransform;
        cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0.5f, 0.5f); cr.sizeDelta = new Vector2(920, 1080);
        VStack(card.gameObject, 30, Pad(60), TextAnchor.UpperCenter);
        Label(card.transform, "Paused", 84, Ink, true, TextAlignmentOptions.Center);
        Label(card.transform, "Your circuit and timer are on hold.", 40, Muted, align: TextAlignmentOptions.Center);
        Spacer(card.transform);
        Button resume = Button(card.transform, "Resume Button", "Resume", Style.Primary);
        Button move = Button(card.transform, "Move Bench Button", "Move the bench", Style.Secondary);
        Button restart = Button(card.transform, "Restart Button", "Restart lab", Style.Secondary);
        Button menu = Button(card.transform, "Menu Button", "Main menu", Style.Secondary);

        var v = Add<PauseView>(s, app);
        Set(v, "resumeButton", resume); Set(v, "moveBenchButton", move);
        Set(v, "restartButton", restart); Set(v, "menuButton", menu);
        return s;
    }

    // ------------------------------------------------------------------ Summary

    static GameObject BuildSummary(Transform parent, AppStateManager app)
    {
        GameObject s = FullScreen(parent, "Summary Screen", true);
        RectTransform col = Column(s, Pad(56, 56, 120, 70), 40);

        RectTransform bulbs = Row(col, "Bulbs", 90, TextAnchor.MiddleCenter);
        for (int i = 0; i < 3; i++) Dot(bulbs, "Bulb " + (i + 1), Sun, 130);
        TextMeshProUGUI headline = Label(col, "Lab complete!", 96, Ink, true, TextAlignmentOptions.Center, "Headline");
        Label(col, "Here's how you did.", 44, Muted, align: TextAlignmentOptions.Center);

        Image scoreCard = CardBox(col, "Score Card");
        VStack(scoreCard.gameObject, 0, Pad(40), TextAnchor.MiddleCenter);
        TextMeshProUGUI score = Label(scoreCard.transform, "0", 150, Ink, true, TextAlignmentOptions.Center, "Score");
        Label(scoreCard.transform, "points", 40, Muted, align: TextAlignmentOptions.Center);

        RectTransform stats = Rect("Stats", col);
        var g = HStack(stats.gameObject, 24, Pad(0)); g.childForceExpandWidth = true;
        TextMeshProUGUI time = Stat(stats, "Time");
        TextMeshProUGUI experiments = Stat(stats, "Experiments");
        TextMeshProUGUI mistakes = Stat(stats, "Mistakes");

        Image saved = Box(col, "Saved", MintTint);
        HStack(saved.gameObject, 0, Pad(40, 28), TextAnchor.MiddleCenter);
        Label(saved.transform, "Saved to your history", 40, MintInk, true, TextAlignmentOptions.Center);

        Spacer(col);
        Button restart = Button(col, "Restart Button", "Restart lab", Style.Primary);
        Button menu = Button(col, "Menu Button", "Main menu", Style.Secondary);

        var v = Add<SummaryView>(s, app);
        Set(v, "headlineText", headline); Set(v, "scoreText", score); Set(v, "timeText", time);
        Set(v, "experimentsText", experiments); Set(v, "mistakesText", mistakes);
        Set(v, "restartButton", restart); Set(v, "menuButton", menu);
        return s;
    }

    static TextMeshProUGUI Stat(Transform parent, string label)
    {
        Image card = CardBox(parent, label);
        VStack(card.gameObject, 4, Pad(20, 34), TextAnchor.MiddleCenter);
        Size(card, flexW: 1);
        TextMeshProUGUI value = Label(card.transform, "-", 58, Ink, true, TextAlignmentOptions.Center, "Value");
        Label(card.transform, label, 34, Muted, align: TextAlignmentOptions.Center);
        return value;
    }

    // ------------------------------------------------------------------ History

    static GameObject BuildHistory(Transform parent, AppStateManager app)
    {
        GameObject s = FullScreen(parent, "History Screen", true);
        RectTransform col = Column(s, Pad(56, 56, 60, 70), 36);

        RectTransform header = Row(col, "Header", 28);
        Button back = RoundButton(header, "Back Button", "‹");
        Label(header, "History", 80, Ink, true);

        Image bestCard = Box(col, "Best Card", MintTint);
        VStack(bestCard.gameObject, 4, Pad(48, 40));
        Label(bestCard.transform, "Best score", 38, MintInk);
        TextMeshProUGUI best = Label(bestCard.transform, "No labs yet", 76, MintInk, true, name: "Best");

        Label(col, "Your last 5 labs", 50, Ink, true);
        Image list = CardBox(col, "Session List");
        VStack(list.gameObject, 0, Pad(0, 10));

        var view = Add<HistoryView>(s, app);
        var so = new UnityEditor.SerializedObject(view);
        var rows = so.FindProperty("rows");
        rows.arraySize = 5;
        for (int i = 0; i < 5; i++)
        {
            RectTransform row = Rect("Row " + (i + 1), list.transform);
            HStack(row.gameObject, 20, Pad(44, 30), TextAnchor.MiddleLeft);
            RectTransform texts = Rect("Texts", row); VStack(texts.gameObject, 4, Pad(0)); Size(texts, flexW: 1);
            TextMeshProUGUI date = Label(texts, "Today, 18:42", 42, Ink, true, name: "Date");
            TextMeshProUGUI detail = Label(texts, "3 of 3 done", 34, Muted, name: "Detail");
            TextMeshProUGUI sc = Label(row, "0", 58, Ink, true, TextAlignmentOptions.Right, "Score");

            var e = rows.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("root").objectReferenceValue = row.gameObject;
            e.FindPropertyRelative("dateText").objectReferenceValue = date;
            e.FindPropertyRelative("detailText").objectReferenceValue = detail;
            e.FindPropertyRelative("scoreText").objectReferenceValue = sc;
        }
        TextMeshProUGUI empty = Label(list.transform, "Finish a lab and it will show up here.", 40, Muted, align: TextAlignmentOptions.Center, name: "Empty");
        empty.margin = new Vector4(40, 40, 40, 40);
        so.FindProperty("backButton").objectReferenceValue = back;
        so.FindProperty("bestText").objectReferenceValue = best;
        so.FindProperty("emptyText").objectReferenceValue = empty.gameObject;
        so.ApplyModifiedPropertiesWithoutUndo();

        Spacer(col);
        Label(col, "Your labs are saved on this phone.", 36, Muted, align: TextAlignmentOptions.Center);
        return s;
    }
}
