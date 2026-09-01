// Run via: CityOfLies > Build Select Case Screen
// Open SelectCaseScreen.unity first (File > New Scene, save it), then run this menu item.
// After running: assign CaseData[] and the tab prefab on the CaseManager GameObject.

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public static class SelectCaseScreenBuilder
{
    // ── Placeholder palette ───────────────────────────────────────────────────
    // ART FLAG: every color below is a stand-in. Search "ART FLAG" for all spots
    // that will benefit from real sprites / assets when art is ready.
    static readonly Color C_BG      = Hex("1D1D2C"); // dark charcoal background
    static readonly Color C_TICKET  = Hex("F0EDE8"); // cream ticket body
    static readonly Color C_DIVIDER = Hex("C8C3B8"); // grey vertical divider line
    static readonly Color C_PHOTO   = Hex("D0CBC5"); // grey evidence photo placeholders
    static readonly Color C_TEXT    = Hex("1A1A1A"); // near-black body text
    static readonly Color C_CONFIRM = Hex("2D6B4F"); // dark green confirm button
    static readonly Color C_BACK    = Hex("E2DED8"); // light grey back button

    [MenuItem("CityOfLies/Build Select Case Screen")]
    static void Build()
    {
        EnsureEventSystem();

        // ── Canvas (1920×1080 reference, scale-with-screen) ───────────────────
        var canvas = BuildCanvas();

        // ── Full-screen background ────────────────────────────────────────────
        Stretch(Img(canvas, "Background", C_BG));

        // ── Title (sits above the ticket) ─────────────────────────────────────
        // ART FLAG: swap to hand-drawn / detective-theme font when ready.
        var titleGO = Tmp(canvas, "TitleText", "SELECT CASE", 54, Color.white);
        CR(titleGO, 0, 220, 700, 68);

        // ── Layout root — ticket and tab stack sit side-by-side inside here ───
        var root = RT(canvas, "CaseScreenRoot");
        CR(root, 0, -10, 1060, 580);

        // ── Ticket panel ──────────────────────────────────────────────────────
        // Positioned so its right edge lands at root-space x = +370.
        // TabStack left edge is at +375, giving a 5px overlap (tab "attached" look).
        // ART FLAG: ticket is a plain cream rect. Replace Source Image with a
        // rounded-corner / torn-edge sprite and set Image Type = Sliced.
        var ticket = Img(root, "TicketPanel", C_TICKET);
        CR(ticket, -80, 0, 900, 580);

        // ── Left column — Evidence photos ─────────────────────────────────────
        var evLabel = Tmp(ticket, "EvidenceLabel", "EVIDENCE", 17, C_TEXT);
        TLR(evLabel, 30, -28, 350, 26);

        var photos = RT(ticket, "PhotosStack");
        TLR(photos, 30, -66, 310, 190);
        // No LayoutGroup — photos overlap intentionally, matching the sketch.
        // ART FLAG: replace color with actual case thumbnail sprites when ready.
        var p1 = Img(photos, "Photo1", C_PHOTO); TLR(p1, 0,  0,   155, 128);
        var p2 = Img(photos, "Photo2", C_PHOTO); TLR(p2, 28, -22, 138, 112);
        // Photo2 renders on top. Swap sibling order in Hierarchy if you want Photo1 on top.

        // ── Left column — Info text ───────────────────────────────────────────
        var infoLabel = Tmp(ticket, "InfoLabel", "INFO", 15, C_TEXT);
        TLR(infoLabel, 30, -276, 280, 24);

        var infoTextGO = Tmp(ticket, "InfoText", "Select a case.", 12, C_TEXT);
        TLR(infoTextGO, 30, -308, 375, 135);
        var infoTmp = infoTextGO.GetComponent<TextMeshProUGUI>();
        infoTmp.alignment    = TextAlignmentOptions.TopLeft;
        infoTmp.overflowMode = TextOverflowModes.Ellipsis;

        // ── Vertical divider (50% of ticket width, 88% of height) ────────────
        var div = Img(ticket, "Divider", C_DIVIDER);
        SetAnchors(div, 0.5f, 0.06f, 0.5f, 0.94f);
        div.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 0);

        // ── Right column — Objectives ─────────────────────────────────────────
        var objLabel = Tmp(ticket, "ObjectivesLabel", "OBJECTIVES", 17, C_TEXT);
        TLR(objLabel, 478, -28, 370, 26);

        var objTextGO = Tmp(ticket, "ObjectivesText",
            "— Gather evidence at the scene\n— Speak to each suspect\n— Select the right clue",
            12, C_TEXT);
        TLR(objTextGO, 478, -66, 370, 230);
        var objTmp = objTextGO.GetComponent<TextMeshProUGUI>();
        objTmp.alignment    = TextAlignmentOptions.TopLeft;
        objTmp.overflowMode = TextOverflowModes.Ellipsis;

        // ── Locked notice (hidden; shown when a locked case tab is clicked) ───
        var lockedGO = RT(ticket, "LockedNotice");
        TLR(lockedGO, 210, -258, 300, 44);
        var lockedTmp      = lockedGO.AddComponent<TextMeshProUGUI>();
        lockedTmp.text      = "COMING SOON";
        lockedTmp.fontSize  = 22;
        lockedTmp.color     = C_TEXT;
        lockedTmp.alignment = TextAlignmentOptions.Center;
        lockedTmp.raycastTarget = false;
        lockedGO.SetActive(false);

        // ── Buttons (bottom-right of ticket) ─────────────────────────────────
        var btnRow = RT(ticket, "ButtonRow");
        BR(btnRow, -28, 28, 346, 56);
        var hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 14;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.MiddleRight;

        var backBtn    = Btn(btnRow, "BackButton",    "BACK",    C_TEXT,      C_BACK,    148, 50);
        var confirmBtn = Btn(btnRow, "ConfirmButton", "CONFIRM", Color.white, C_CONFIRM, 178, 50);

        // ── Tab stack (right of ticket, slightly overlapping its right edge) ──
        // ART FLAG: tabs are plain white/terracotta rects. Replace tabBackground
        // sprite in the CaseEntryUI prefab with rounded-corner art when ready.
        // The ticket "notch" where tabs attach also benefits from a sprite mask.
        var tabStack = RT(root, "TabStack");
        CR(tabStack, 443, 0, 136, 548);
        var vlg = tabStack.AddComponent<VerticalLayoutGroup>();
        vlg.spacing              = 10;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment       = TextAnchor.UpperCenter;

        // ── CaseSelectionManager ──────────────────────────────────────────────
        var mgrGO = new GameObject("CaseManager");
        mgrGO.transform.SetParent(canvas.transform, false);
        var mgr = mgrGO.AddComponent<CaseSelectionManager>();

        mgr.caseListParent   = tabStack.GetComponent<RectTransform>();
        mgr.detailDescription = infoTmp;
        mgr.objectivesText    = objTmp;
        mgr.lockedNotice      = lockedGO;
        mgr.confirmButton     = confirmBtn.GetComponent<Button>();
        // detailCaseName left null — no separate case-name label in this layout.

        Selection.activeGameObject = mgrGO;
        Debug.Log(
            "[SelectCaseScreenBuilder] Done!\n\n" +
            "Still needed on CaseManager in the Inspector:\n" +
            "  Cases[]          — run CityOfLies > Create Case Assets first, then drag\n" +
            "                     CityOfLiesCase, ComingSoonSlot1, ComingSoonSlot2\n" +
            "  Case Entry Prefab — drag the CaseEntryUI tab prefab\n\n" +
            "Wire onClick on the buttons:\n" +
            "  BackButton    > On Click() > CaseManager.OnBackClicked()\n" +
            "  ConfirmButton > On Click() > CaseManager.OnConfirmClicked()\n\n" +
            "Tab prefab structure is documented at the top of CaseEntryUI.cs.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Case asset creation
    // ═════════════════════════════════════════════════════════════════════════

    // Creates the three CaseData assets under Assets/ScriptableObjects/Cases/.
    // Run this once before opening the scene — or at any time to regenerate them.
    [MenuItem("CityOfLies/Create Case Assets")]
    static void CreateCaseAssets()
    {
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Cases"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Cases");

        // ── The one real, playable case ───────────────────────────────────────
        CreateOrReplaceAsset<CaseData>(
            "Assets/ScriptableObjects/Cases/CityOfLiesCase.asset",
            a =>
            {
                a.caseID       = "cityoflies";
                a.caseName     = "City of Lies";
                a.description  = "Julian Reyes is dead, and everyone close to him has something to hide. " +
                                 "As Chief Inspector Marcus Doyle, uncover the truth behind the murder — " +
                                 "one suspect, one lie at a time.";
                a.startingScene = "ExGF_Diner_P1";
                a.isUnlocked   = true;
            });

        // ── Placeholder locked slots (cases[1] and cases[2] in the tab stack) ─
        // These are not real cases — they keep the three-tab visual intact.
        // When a future case is ready, replace these with real CaseData assets.
        CreateOrReplaceAsset<CaseData>(
            "Assets/ScriptableObjects/Cases/ComingSoonSlot1.asset",
            a => { a.caseName = "Coming Soon"; a.isUnlocked = false; });

        CreateOrReplaceAsset<CaseData>(
            "Assets/ScriptableObjects/Cases/ComingSoonSlot2.asset",
            a => { a.caseName = "Coming Soon"; a.isUnlocked = false; });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<CaseData>(
            "Assets/ScriptableObjects/Cases/CityOfLiesCase.asset");

        Debug.Log("[SelectCaseScreenBuilder] Case assets created in Assets/ScriptableObjects/Cases/.\n" +
                  "Drag CityOfLiesCase, ComingSoonSlot1, ComingSoonSlot2 into CaseManager.Cases[] in that order.");
    }

    static void CreateOrReplaceAsset<T>(string path, System.Action<T> configure) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null)
        {
            configure(existing);
            EditorUtility.SetDirty(existing);
        }
        else
        {
            var asset = ScriptableObject.CreateInstance<T>();
            configure(asset);
            AssetDatabase.CreateAsset(asset, path);
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Factory helpers — all return GameObject so callers can pass them to
    //  the RectTransform setters without .gameObject everywhere.
    // ═════════════════════════════════════════════════════════════════════════

    static GameObject BuildCanvas()
    {
        var go = new GameObject("Canvas");
        var c  = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    // Plain RectTransform (no visual component)
    static GameObject RT(GameObject parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    // Image component — returns the GO so it can be passed to layout helpers
    static GameObject Img(GameObject parent, string name, Color col)
    {
        var go = RT(parent, name);
        go.AddComponent<Image>().color = col;
        return go;
    }

    // TextMeshProUGUI — returns the GO; caller uses GetComponent<TextMeshProUGUI>() if needed
    static GameObject Tmp(GameObject parent, string name, string text, float size, Color col)
    {
        var go  = RT(parent, name);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = col; tmp.raycastTarget = false;
        return go;
    }

    // Button with a Label child — returns the GO
    static GameObject Btn(GameObject parent, string name, string label, Color textCol, Color bgCol, float w, float h)
    {
        var go = RT(parent, name);
        go.AddComponent<Image>().color = bgCol;
        go.AddComponent<Button>();
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = w; le.preferredHeight = h;

        var lblGO = RT(go, "Label");
        var lt    = lblGO.AddComponent<TextMeshProUGUI>();
        lt.text = label; lt.fontSize = 16; lt.color = textCol;
        lt.alignment = TextAlignmentOptions.Center; lt.raycastTarget = false;
        Stretch(lblGO);
        return go;
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  RectTransform setters — all take GameObject
    // ═════════════════════════════════════════════════════════════════════════

    // Center-anchored: x/y offset from parent center, fixed w/h
    static void CR(GameObject go, float x, float y, float w, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta        = new Vector2(w, h);
    }

    // Top-left anchored: x from left, y negative from top, fixed w/h
    static void TLR(GameObject go, float x, float y, float w, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot            = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta        = new Vector2(w, h);
    }

    // Bottom-right anchored: x/y inset from bottom-right corner, fixed w/h
    static void BR(GameObject go, float x, float y, float w, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot            = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta        = new Vector2(w, h);
    }

    // Normalized anchor stretch (for the divider line)
    static void SetAnchors(GameObject go, float xMin, float yMin, float xMax, float yMax)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // Full-stretch: fills parent in all directions
    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color c);
        return c;
    }
}
