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
            setup = Dark(() => BuildSetup(safe, app, placement)),
            hud = Dark(() => BuildHud(safe, app)),
            pause = BuildPause(safe, app),
            summary = BuildSummary(safe, app),
            history = BuildHistory(safe, app),
        };
    }

    // ------------------------------------------------------------------ helpers

    // Camera screens are built with the dark glass palette.
    static GameObject Dark(System.Func<GameObject> build)
    {
        using (DarkGlass()) return build();
    }

    static GameObject FullScreen(Transform parent, string name, bool background)
    {
        RectTransform rt = Rect(name, parent); Stretch(rt);
        if (background) FullBleed(rt, "Background", Bg);
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
        RectTransform col = Column(s, Pad(56, 56, 50, 60), 26);

        RectTransform header = Row(col, "Header", 28);
        Label(header, "AR Circuits Lab", 60, Ink, true);

        Image hero = CardBox(col, "Hero Card");
        VStack(hero.gameObject, 22, Pad(48, 40));
        Image art = Box(hero.transform, "Illustration", SunTint);
        Size(art, 200);
        HStack(art.gameObject, 40, Pad(40), TextAnchor.MiddleCenter);
        CircuitPicture(art.transform, 130);
        Title(hero.transform, "Light up your first circuit", 66);
        Body(hero.transform, "Build real circuits on your desk with your phone's camera. No kit needed.", 40);
        Button start = Button(hero.transform, "Start Button", "Start 5 experiments", Style.Primary);
        Button freeBuild = Button(hero.transform, "Free Build Button", "Free build (no points)", Style.Secondary);

        Label(col, "Your experiments", 48, Ink, true);
        Button[] rows =
        {
            ExperimentRow(col, "1", "Light the bulb", "Start here", true),
            ExperimentRow(col, "2", "Dim the bulb", "Resistor", false),
            ExperimentRow(col, "3", "Two bulbs in series", "Series", false),
            ExperimentRow(col, "4", "Two bulbs in parallel", "Parallel", false),
            ExperimentRow(col, "5", "Fix the broken circuit", "Fault", false),
        };

        Spacer(col);
        Image bestRow = Box(col, "Best Score", MintTint);
        var best = bestRow.gameObject.AddComponent<UnityEngine.UI.Button>(); best.targetGraphic = bestRow;
        HStack(bestRow.gameObject, 28, Pad(40, 32), TextAnchor.MiddleLeft);
        Badge(bestRow.transform, "Trophy", "1st", Mint, Hex("04281D"), 100);
        RectTransform bestTexts = Rect("Texts", bestRow.transform); VStack(bestTexts.gameObject, 4, Pad(0)); Size(bestTexts, flexW: 1);
        Label(bestTexts, "Your best score", 36, MintInk);
        TextMeshProUGUI bestScore = Label(bestTexts, "No labs yet", 50, MintInk, true, name: "Best Score Text");
        Label(bestRow.transform, "History", 42, MintInk, true);

        var v = Add<HomeView>(s, app);
        Set(v, "startButton", start); Set(v, "freeBuildButton", freeBuild); Set(v, "historyButton", best);
        var hso = new UnityEditor.SerializedObject(v);
        var list = hso.FindProperty("experimentButtons");
        list.arraySize = rows.Length;
        for (int i = 0; i < rows.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = rows[i];
        hso.ApplyModifiedPropertiesWithoutUndo(); Set(v, "bestScoreText", bestScore);
        return s;
    }

    // Battery, switch and bulb icons in a row: a simple picture of a circuit.
    static void CircuitPicture(Transform parent, float size)
    {
        foreach (string part in new[] { "Battery", "Switch", "Bulb" })
        {
            Image icon = Rect(part, parent).gameObject.AddComponent<Image>();
            icon.sprite = Icon(part);
            icon.color = part == "Bulb" ? Sun : part == "Switch" ? Orange : Ink;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            Size(icon, size, size);
        }
    }

    static Button ExperimentRow(Transform parent, string number, string title, string tag, bool active)
    {
        Image row = CardBox(parent, "Experiment " + number, Card, active ? Sun : Border);
        var button = row.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = row;
        HStack(row.gameObject, 26, Pad(30, 16), TextAnchor.MiddleLeft);
        Badge(row.transform, "Number", number, active ? Sun : Soft, active ? OnSun : Muted, 76);
        TextMeshProUGUI name = Label(row.transform, title, 42, Ink, true); Size(name, flexW: 1);
        name.textWrappingMode = TextWrappingModes.NoWrap;
        Image chip = Box(row.transform, "Tag", active ? SunTint : Soft, RoundedSmall, 0.5f);
        HStack(chip.gameObject, 0, Pad(26, 12), TextAnchor.MiddleCenter);
        Label(chip.transform, tag, 32, active ? SunInk : Muted, true).textWrappingMode = TextWrappingModes.NoWrap;
        Label(row.transform, "›", 56, Muted, true);
        return button;
    }

    // ------------------------------------------------------------------ Briefing

    static GameObject BuildBriefing(Transform parent, AppStateManager app)
    {
        GameObject s = FullScreen(parent, "Briefing Screen", true);
        RectTransform col = Column(s, Pad(56, 56, 60, 70), 44);

        RectTransform header = Row(col, "Header", 28);
        Button back = RoundButton(header, "Back Button", "‹");
        TextMeshProUGUI number = Label(header, "Experiment 1 of 5", 40, Muted, name: "Number");

        Image art = CardBox(col, "Illustration");
        Size(art, 500);
        HStack(art.gameObject, 50, Pad(60), TextAnchor.MiddleCenter);
        CircuitPicture(art.transform, 170);

        TextMeshProUGUI bTitle = Title(col, "Light the bulb", 80);
        TextMeshProUGUI bGoal = Label(col, "Connect a battery, a switch and a bulb in one loop. Then close the switch.", 46, Ink, name: "Goal");

        Label(col, "You'll use", 38, Muted, true);
        RectTransform chips = Row(col, "Parts", 20);
        var chipTexts = new TextMeshProUGUI[4];
        for (int i = 0; i < chipTexts.Length; i++)
        {
            Image chip = CardBox(chips, "Chip " + (i + 1));
            HStack(chip.gameObject, 0, Pad(26, 18), TextAnchor.MiddleCenter);
            chipTexts[i] = Label(chip.transform, "part", 34, Ink, true);
            chipTexts[i].textWrappingMode = TextWrappingModes.NoWrap;
        }

        Image learn = Box(col, "What you'll learn", SunTint);
        VStack(learn.gameObject, 8, Pad(44, 36));
        Label(learn.transform, "What you'll learn", 42, SunInk, true);
        TextMeshProUGUI learnText = Label(learn.transform, "Current only flows when the path is a complete loop.", 40, SunInk, name: "Learn");

        Spacer(col);
        Label(col, "You'll need a table and good light.", 36, Muted, align: TextAlignmentOptions.Center);
        Button find = Button(col, "Find Table Button", "Find a table", Style.Primary);

        var v = Add<BriefingView>(s, app);
        Set(v, "backButton", back); Set(v, "findTableButton", find);
        Set(v, "numberText", number); Set(v, "titleText", bTitle); Set(v, "goalText", bGoal); Set(v, "learnText", learnText);
        var bso = new UnityEditor.SerializedObject(v);
        var chipList = bso.FindProperty("partChips");
        chipList.arraySize = chipTexts.Length;
        for (int i = 0; i < chipTexts.Length; i++) chipList.GetArrayElementAtIndex(i).objectReferenceValue = chipTexts[i];
        bso.ApplyModifiedPropertiesWithoutUndo();
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

        // ---- Top: one compact card with the goal and the live readings, plus pause ----
        RectTransform top = Rect("Top Bar", s.transform);
        Dock(top, true, 0, 32, 40);
        var th = HStack(top.gameObject, 20, Pad(0), TextAnchor.UpperLeft);
        th.childForceExpandHeight = false;
        top.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Image goal = CardBox(top, "Goal Card"); Size(goal, flexW: 1);
        VStack(goal.gameObject, 6, Pad(36, 26));
        TextMeshProUGUI exp = Label(goal.transform, "Experiment 1 of 5", 30, Muted, name: "Experiment");
        TextMeshProUGUI title = Label(goal.transform, "Light the bulb", 46, Ink, true, name: "Title");
        TextMeshProUGUI goalText = Label(goal.transform, "Connect a battery, a switch and a bulb.", 34, Ink, name: "Goal");
        RectTransform readings = Row(goal.transform, "Readings", 14);
        readings.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(0, 0, 10, 0);
        TextMeshProUGUI current = Reading(readings, "Current", "0.00 A");
        TextMeshProUGUI circuit = Reading(readings, "Circuit", "Open");
        TextMeshProUGUI score = Reading(readings, "Score", "0");
        Button pause = RoundButton(top, "Pause Button", "II");

        // ---- Bottom: one row of parts, with Undo at the end ----
        const float barHeight = 220f;
        Image bar = BottomSheet(s.transform, "Parts Bar", barHeight);
        var bl = HStack(bar.gameObject, 14, Pad(28, 28, 22, (int)Bleed + 22), TextAnchor.UpperCenter);
        bl.childForceExpandWidth = true;
        Button battery = Part(bar.transform, "Battery"); Button sw = Part(bar.transform, "Switch");
        Button bulb = Part(bar.transform, "Bulb"); Button resistor = Part(bar.transform, "Resistor");
        Button undo = Part(bar.transform, "Undo");

        // ---- Hint: a small dark note just above the parts bar ----
        Image hint = Box(s.transform, "Hint Bubble", new Color(0.04f, 0.07f, 0.13f, 0.9f));
        RectTransform hr = hint.rectTransform;
        hr.anchorMin = new Vector2(0, 0); hr.anchorMax = new Vector2(1, 0); hr.pivot = new Vector2(0.5f, 0);
        hr.sizeDelta = new Vector2(-96, 0); hr.anchoredPosition = new Vector2(0, barHeight + 24);
        VStack(hint.gameObject, 0, Pad(32, 20), TextAnchor.MiddleCenter);
        hint.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        TextMeshProUGUI hintText = Label(hint.transform, "Tap a part below, then tap the bench.", 32, Color.white,
            align: TextAlignmentOptions.Center, name: "Hint");

        // Well-done card (hidden until an experiment is finished). It sits at the bottom, over
        // the parts drawer, so the finished circuit stays in view above it.
        Image card = CardBox(s.transform, "Success Card", Card, Mint);
        RectTransform cr = card.rectTransform;
        cr.anchorMin = new Vector2(0, 0); cr.anchorMax = new Vector2(1, 0); cr.pivot = new Vector2(0.5f, 0);
        cr.sizeDelta = new Vector2(-64, 0); cr.anchoredPosition = new Vector2(0, 24);
        VStack(card.gameObject, 22, Pad(44, 40), TextAnchor.UpperCenter);
        card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        RectTransform sh = Row(card.transform, "Header", 28);
        TextMeshProUGUI pts = Badge(sh, "Points", "+100", Mint, Hex("04281D"), 140);
        pts.fontSize = 44; pts.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform st = Rect("Texts", sh); VStack(st.gameObject, 2, Pad(0)); Size(st, flexW: 1);
        TextMeshProUGUI sSub = Label(st, "Experiment 1 of 5 complete", 34, MintInk, true, name: "Subtitle");
        TextMeshProUGUI sTitle = Label(st, "Well done!", 60, Ink, true, name: "Title");
        Image why = Box(card.transform, "Why Box", SunTint);
        VStack(why.gameObject, 0, Pad(36, 26));
        TextMeshProUGUI sBody = Label(why.transform, "Closing the switch completed the loop, so current can flow.", 36, SunInk, name: "Body");
        TextMeshProUGUI sNext = Label(card.transform, "Up next: Dim the bulb.", 34, Muted, name: "Next");
        Button cont = Button(card.transform, "Continue Button", "Start experiment 2", Style.Primary);
        GameObject success = card.gameObject;

        // Short-circuit warning (hidden until it happens)
        Image warn = CardBox(s.transform, "Warning Banner", Card, Red);
        Dock(warn.rectTransform, true, 420, 44, 56);
        VStack(warn.gameObject, 22, Pad(44, 36));
        warn.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        RectTransform wh = Row(warn.transform, "Header", 24);
        Badge(wh, "Alert", "!", RedTint, RedInk, 100);
        Label(wh, "Short circuit!", 60, RedInk, true);
        Label(warn.transform, "The battery's two ends are joined with nothing in between. A real battery would get hot.", 40, Ink);
        TextMeshProUGUI penalty = Label(warn.transform, "1 mistake recorded, 10 points lost.", 36, RedInk, true, name: "Penalty");
        Button wUndo = Button(warn.transform, "Undo Wire Button", "Undo wire", Style.Danger, 130, 44);

        Set(v, "experimentText", exp); Set(v, "titleText", title); Set(v, "goalText", goalText);
        Set(v, "pauseButton", pause); Set(v, "currentText", current); Set(v, "circuitText", circuit);
        Set(v, "scoreText", score); Set(v, "hintBubble", hint.gameObject); Set(v, "hintText", hintText);
        Set(v, "batteryButton", battery); Set(v, "switchButton", sw); Set(v, "bulbButton", bulb);
        Set(v, "resistorButton", resistor); Set(v, "undoButton", undo);
        Set(v, "successSheet", success); Set(v, "successTitle", sTitle); Set(v, "successBody", sBody); Set(v, "successNext", sNext);
        Set(v, "successSubtitle", sSub); Set(v, "successPoints", pts); Set(v, "warningPenalty", penalty);
        Set(v, "continueButton", cont); Set(v, "warningBanner", warn.gameObject); Set(v, "warningUndoButton", wUndo);
        return s;
    }

    // A small chip such as "Current 0.25 A".
    static TextMeshProUGUI Reading(Transform parent, string label, string value)
    {
        Image chip = Box(parent, label, Bg, RoundedSmall, 0.5f);
        HStack(chip.gameObject, 10, Pad(20, 12), TextAnchor.MiddleLeft);
        Label(chip.transform, label, 26, Muted).textWrappingMode = TextWrappingModes.NoWrap;
        TextMeshProUGUI v = Label(chip.transform, value, 30, Ink, true, name: "Value");
        v.textWrappingMode = TextWrappingModes.NoWrap;
        return v;
    }

    static Button Part(Transform parent, string name)
    {
        bool isUndo = name == "Undo";
        Image img = CardBox(parent, name + (isUndo ? " Button" : " Part"), isUndo ? Soft : Bg);
        var b = img.gameObject.AddComponent<UnityEngine.UI.Button>(); b.targetGraphic = img;
        var pv = VStack(img.gameObject, 10, Pad(6, 18), TextAnchor.MiddleCenter);
        pv.childForceExpandWidth = false;
        // Same width for every button: no preferred width, share the row equally.
        LayoutElement le = Size(img, 176, flexW: 1);
        le.preferredWidth = 0; le.minWidth = 0;

        // A line icon that looks like the part (circuit-diagram style).
        Image icon = Rect("Icon", img.transform).gameObject.AddComponent<Image>();
        icon.sprite = Icon(name);
        icon.color = name == "Bulb" ? Sun : Ink;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        Size(icon, 76, 76);
        Label(img.transform, name, 28, Ink, true, TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.NoWrap;
        return b;
    }

    // ------------------------------------------------------------------ Pause

    static GameObject BuildPause(Transform parent, AppStateManager app)
    {
        GameObject s = FullScreen(parent, "Pause Screen", false);
        FullBleed(s.transform, "Dim", new Color(0.1f, 0.07f, 0.03f, 0.55f));

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

        RectTransform bulbs = Row(col, "Bulbs", 40, TextAnchor.MiddleCenter);
        for (int i = 0; i < 5; i++) Dot(bulbs, "Bulb " + (i + 1), Sun, 110);
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
            TextMeshProUGUI detail = Label(texts, "5 of 5 done", 34, Muted, name: "Detail");
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
