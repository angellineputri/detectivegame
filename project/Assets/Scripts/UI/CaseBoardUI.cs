using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaseBoardUI : MonoBehaviour
{
    public static CaseBoardUI Instance { get; private set; }

    [Header("Overlay")]
    [SerializeField] CanvasGroup   boardOverlay;
    [SerializeField] Button        closeButton;
    [SerializeField] RectTransform diagramRoot;

    [Header("Animation")]
    [SerializeField] float overlayFadeDuration = 0.35f;
    [SerializeField] float nodeRevealDelay     = 0.12f;

    static readonly Color CardBg       = new Color32(0xe8, 0xdc, 0xc0, 0xff);
    static readonly Color CardInk      = new Color32(0x3a, 0x35, 0x2e, 0xff);
    static readonly Color LabelInk     = new Color32(0x8a, 0x7f, 0x68, 0xff);
    static readonly Color PinBlueLight = new Color32(0x6f, 0xa0, 0xc9, 0xff);
    static readonly Color PinRedLight  = new Color32(0xe0, 0x78, 0x62, 0xff);
    static readonly Color PinGrayLight = new Color32(0x8a, 0x94, 0xa0, 0xff);
    static readonly Color TickRed      = new Color32(0x7e, 0x23, 0x18, 0xff);
    static readonly Color StringRed    = new Color32(0xa5, 0x3f, 0x34, 0xff);
    static readonly Color StringMuted  = new Color32(0x7a, 0x71, 0x60, 0xff);
    static readonly Color StartPin     = new Color32(0xd9, 0x58, 0x4a, 0xff);
    static readonly Color PinStem      = new Color32(0x28, 0x1e, 0x18, 0xe8);

    NodeCard _slot1A, _slot2A, _slot3A;
    NodeCard _slot1B, _slot2B, _slot3B;
    NodeCard _merged;

    LineGroup _lnStartA, _lnStartB;
    LineGroup _lnA12,    _lnB12;
    LineGroup _lnA23,    _lnB23;
    LineGroup _lnMergeA, _lnMergeB;

    bool      _isVisible;
    bool      _subscribed;
    Coroutine _showCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (boardOverlay != null)
        {
            boardOverlay.alpha          = 0f;
            boardOverlay.interactable   = false;
            boardOverlay.blocksRaycasts = false;
        }
        closeButton?.onClick.AddListener(DismissBoard);

        if (closeButton != null)
        {
            TMP_FontAsset ps2p = Resources.Load<TMP_FontAsset>("Fonts & Materials/PressStart2P SDF");
            TMP_Text closeLbl = closeButton.GetComponentInChildren<TMP_Text>();
            if (closeLbl == null)
            {
                GameObject lblGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                lblGO.transform.SetParent(closeButton.transform, false);
                RectTransform rt = lblGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                closeLbl = lblGO.GetComponent<TextMeshProUGUI>();
                closeLbl.alignment = TextAlignmentOptions.Center;
            }
            closeLbl.text = "X";
            closeLbl.fontSize = 16;
            if (ps2p != null) closeLbl.font = ps2p;
        }

        BuildDiagram();
    }

    void Start()     => TrySubscribe();
    void OnEnable()  => TrySubscribe();
    void OnDisable() => TryUnsubscribe();
    void OnDestroy() => TryUnsubscribe();

    void Update()
    {
        if (_isVisible && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            DismissBoard();
    }

    void TrySubscribe()
    {
        if (_subscribed || CaseBoardManager.Instance == null) return;
        CaseBoardManager.Instance.OnBoardChanged += HandleBoardChanged;
        _subscribed = true;
    }

    void TryUnsubscribe()
    {
        if (!_subscribed || CaseBoardManager.Instance == null) return;
        CaseBoardManager.Instance.OnBoardChanged -= HandleBoardChanged;
        _subscribed = false;
    }

    void HandleBoardChanged()
    {
        if (_showCoroutine != null) StopCoroutine(_showCoroutine);
        _showCoroutine = StartCoroutine(ShowAndUpdate());
    }

    public bool IsBoardVisible => _isVisible;

    public void ShowFinalReveal(string culpritName = null)
        => CaseBoardManager.Instance?.MarkP3Done();

    IEnumerator ShowAndUpdate()
    {
        if (!_isVisible)
        {
            _isVisible = true;
            if (PlayerController.Instance != null) PlayerController.Instance.CanMove = false;
            BagUI.Instance?.SetHudVisible(false);
            yield return StartCoroutine(FadeOverlay(0f, 1f));
        }

        var bm = CaseBoardManager.Instance;
        if (bm == null) yield break;

        yield return StartCoroutine(RefreshDiagram(bm));
    }

    IEnumerator RefreshDiagram(CaseBoardManager bm)
    {
        int sA = bm.stageA;
        int sB = bm.stageB;

        bool exGfDead = sA >= 5;

        bool asstDead = bm.assistantP1CourtDone;

        bool merged          = exGfDead && asstDead;
        bool culpritRevealed = bm.p3Done;

        bool slot3AVisible = exGfDead && !merged;
        bool slot2BDead    = asstDead && !merged;

        _lnStartA.Set(sA >= 1, true);
        _lnA12.Set(sA >= 3, sA >= 3);
        _lnA23.Set(true, slot3AVisible);

        _lnStartB.Set(sB >= 1, true);
        _lnB12.Set(sB >= 3, slot2BDead);
        _lnB23.Set(false, false);

        _lnMergeA.Set(true, merged);
        _lnMergeB.Set(true, merged);

        if (sA == 0)
            _slot1A.ShowPlaceholder();
        else
            _slot1A.ShowNode(sA >= 2,
                sA >= 2 ? bm.exGfChain?.stage1ArrestedName
                         : bm.exGfChain?.stage1SuspectName);

        bool was2A = _slot2A.IsVisible;
        _slot2A.SetActive(sA >= 3);
        if (sA >= 3)
        {
            _slot2A.ShowNode(sA >= 4,
                sA >= 4 ? bm.exGfChain?.stage2ArrestedName
                         : bm.exGfChain?.stage2SuspectName);
            if (!was2A) yield return StartCoroutine(Reveal(_slot2A.cg));
        }

        bool was3A = _slot3A.IsVisible;
        _slot3A.SetActive(slot3AVisible);
        if (slot3AVisible)
        {
            _slot3A.ShowDeadEnd();
            if (!was3A) yield return StartCoroutine(Reveal(_slot3A.cg));
        }

        if (sB == 0)
            _slot1B.ShowPlaceholder();
        else
            _slot1B.ShowNode(sB >= 2,
                sB >= 2 ? bm.assistantChain?.stage1ArrestedName
                         : bm.assistantChain?.stage1SuspectName);

        bool was2B = _slot2B.IsVisible;
        _slot2B.SetActive(slot2BDead);
        if (slot2BDead)
        {
            _slot2B.ShowDeadEnd();
            if (!was2B) yield return StartCoroutine(Reveal(_slot2B.cg));
        }

        _slot3B.SetActive(false);

        bool wasMerged = _merged.IsVisible;
        _merged.SetActive(merged);
        if (merged)
        {
            _merged.ShowMerged(culpritRevealed, culpritRevealed ? bm.culpritName : "???");
            if (!wasMerged) yield return StartCoroutine(Reveal(_merged.cg));
        }

        RecenterDiagram(merged, slot3AVisible, sA >= 3 || slot2BDead);
    }

    void DismissBoard()
    {
        if (!_isVisible) return;
        if (_showCoroutine != null) StopCoroutine(_showCoroutine);
        StartCoroutine(DismissCoroutine());
    }

    IEnumerator DismissCoroutine()
    {
        yield return StartCoroutine(FadeOverlay(1f, 0f));
        _isVisible = false;
        if (PlayerController.Instance != null) PlayerController.Instance.CanMove = true;
        BagUI.Instance?.RefreshBagHudVisibility();
    }

    IEnumerator FadeOverlay(float from, float to)
    {
        if (boardOverlay == null) yield break;
        if (to <= 0f) { boardOverlay.interactable = false; boardOverlay.blocksRaycasts = false; }
        for (float t = 0f; t < overlayFadeDuration; t += Time.deltaTime)
        {
            boardOverlay.alpha = Mathf.Lerp(from, to, t / overlayFadeDuration);
            yield return null;
        }
        boardOverlay.alpha = to;
        if (to >= 1f) { boardOverlay.interactable = true; boardOverlay.blocksRaycasts = true; }
    }

    IEnumerator Reveal(CanvasGroup cg)
    {
        if (cg == null) yield break;
        yield return new WaitForSeconds(nodeRevealDelay);

        var rt = (RectTransform)cg.transform;
        cg.alpha      = 0f;
        rt.localScale = Vector3.one * 0.85f;

        const float dur = 0.3f;
        for (float t = 0f; t < dur; t += Time.deltaTime)
        {
            float p       = t / dur;
            cg.alpha      = p;
            rt.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one * 1.05f, p);
            yield return null;
        }
        cg.alpha      = 1f;
        rt.localScale = Vector3.one;
    }

    void RecenterDiagram(bool merged, bool slot3Visible, bool slot2Visible)
    {
        if (diagramRoot == null) return;
        float rightEdge = merged ? 1506f : slot3Visible ? 1236f : slot2Visible ? 921f : 561f;
        float scale = diagramRoot.localScale.x;
        float cx = (30f + rightEdge) * 0.5f * scale;
        float cy = 300f * scale;
        diagramRoot.anchoredPosition = new Vector2(-cx, cy);
    }

    void BuildDiagram()
    {
        if (diagramRoot == null)
        {
            Debug.LogWarning("[CaseBoardUI] diagramRoot not assigned — run Tools → City of Lies UI → Build Case Board.");
            return;
        }

        diagramRoot.anchorMin = diagramRoot.anchorMax = new Vector2(0.5f, 0.5f);
        diagramRoot.pivot     = new Vector2(0f, 1f);

        if (diagramRoot.childCount > 0) return;

        var head = TryLoadFont("Fonts & Materials/PressStart2P SDF");
        var body = TryLoadFont("Fonts & Materials/VT323 SDF");

        _lnStartA = MakeBezierLine("LineStartA", new Vector2(195,315), new Vector2(262.5f,232.5f), new Vector2(360,165));
        _lnStartB = MakeBezierLine("LineStartB", new Vector2(195,315), new Vector2(262.5f,397.5f), new Vector2(360,435));
        _lnA12    = MakeBezierLine("LineA12",    new Vector2(555,150), new Vector2(637.5f,138),     new Vector2(720,150));
        _lnB12    = MakeBezierLine("LineB12",    new Vector2(555,435), new Vector2(637.5f,447),     new Vector2(720,435));
        _lnA23    = MakeBezierLine("LineA23",    new Vector2(915,150), new Vector2(997.5f,132),     new Vector2(1080,150));
        _lnB23    = MakeBezierLine("LineB23",    new Vector2(915,435), new Vector2(997.5f,453),     new Vector2(1080,435));
        _lnMergeA = MakeBezierLine("LineMergeA", new Vector2(915,150), new Vector2(1140,195),      new Vector2(1290,307.5f));
        _lnMergeB = MakeBezierLine("LineMergeB", new Vector2(555,435), new Vector2(920,390),       new Vector2(1290,315));

        MakeStartCard(30, 270, 165, 96, 1f, body);

        _slot1A = MakeNodeCard("Slot1A",  360, 117, 201, 96,  1.5f, head, body);
        _slot2A = MakeNodeCard("Slot2A",  720, 117, 201, 96, -1.5f, head, body);
        _slot3A = MakeNodeCard("Slot3A", 1080, 117, 156, 96,  1f,   head, body);

        _slot1B = MakeNodeCard("Slot1B",  360, 387, 201, 96, -1.5f, head, body);
        _slot2B = MakeNodeCard("Slot2B",  720, 387, 201, 96,  1.5f, head, body);
        _slot3B = MakeNodeCard("Slot3B", 1080, 387, 156, 96, -1f,   head, body);

        _merged  = MakeNodeCard("Merged", 1290, 255, 216, 108, 1f,  head, body);

        if (_merged.value != null) _merged.value.enableWordWrapping = true;

        _slot1A.ShowPlaceholder(); _slot1B.ShowPlaceholder();
        _slot2A.SetActive(false);  _slot2B.SetActive(false);
        _slot3A.SetActive(false);  _slot3B.SetActive(false);
        _merged.SetActive(false);

        _lnStartA.Set(false, true);  _lnStartB.Set(false, true);
        _lnA12.Set(false, false);    _lnB12.Set(false, false);
        _lnA23.Set(false, false);    _lnB23.Set(false, false);
        _lnMergeA.Set(false, false); _lnMergeB.Set(false, false);

        diagramRoot.localScale = new Vector3(1.1f, 1.1f, 1f);
        RecenterDiagram(false, false, false);
    }

    NodeCard MakeNodeCard(string name, float x, float y, float w, float h, float cssRotateDeg,
                          TMP_FontAsset head, TMP_FontAsset body)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        go.transform.SetParent(diagramRoot, false);
        PlaceTopLeft((RectTransform)go.transform, x, y, w, h);
        go.GetComponent<Image>().color = CardBg;
        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor    = new Color(0f, 0f, 0f, 0.50f);
        shadow.effectDistance = new Vector2(5f, -6f);
        go.transform.localEulerAngles = new Vector3(0f, 0f, -cssRotateDeg);
        var cg = go.GetComponent<CanvasGroup>();

        var pin = MakePinCompound(go.transform, PinBlueLight);

        var ph = MakeTMP(go.transform, "Placeholder", "???", 30f, LabelInk, body);
        Stretch((RectTransform)ph.transform);
        ph.alignment = TextAlignmentOptions.Center;

        var content   = MakeRect(go.transform, "Content");
        var contentRt = (RectTransform)content.transform;
        contentRt.anchorMin = Vector2.zero;
        contentRt.anchorMax = Vector2.one;
        contentRt.offsetMin = new Vector2(12f, 9f);
        contentRt.offsetMax = new Vector2(-9f, -20f);

        var tick   = MakeTMP(content.transform, "Tick", "✓", 21f, TickRed, body);
        var tickRt = (RectTransform)tick.transform;
        tickRt.anchorMin        = new Vector2(0f, 0.5f);
        tickRt.anchorMax        = new Vector2(0f, 0.5f);
        tickRt.pivot            = new Vector2(0f, 0.5f);
        tickRt.anchoredPosition = Vector2.zero;
        tickRt.sizeDelta        = new Vector2(27f, 27f);
        tick.alignment          = TextAlignmentOptions.Center;

        var tb   = MakeRect(content.transform, "TextBlock");
        var tbRt = (RectTransform)tb.transform;
        tbRt.anchorMin = Vector2.zero;
        tbRt.anchorMax = Vector2.one;
        tbRt.offsetMin = new Vector2(33f, 0f);
        tbRt.offsetMax = Vector2.zero;
        var vlg = tb.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;
        vlg.spacing = 1.5f;

        var lbl = MakeTMP(tb.transform, "Label", "", 12f, LabelInk, head);
        lbl.fontStyle = FontStyles.UpperCase;

        var val = MakeTMP(tb.transform, "Value", "???", 19.5f, CardInk, body);
        val.fontStyle          = FontStyles.Bold;
        val.enableWordWrapping = false;

        return new NodeCard
        {
            root        = (RectTransform)go.transform,
            pinRoot     = pin.transform.parent.gameObject,
            pin         = pin,
            placeholder = ph,
            content     = content,
            tick        = tick,
            label       = lbl,
            value       = val,
            cg          = cg
        };
    }

    void MakeStartCard(float x, float y, float w, float h, float unityZRot, TMP_FontAsset font)
    {
        var go = new GameObject("StartCard", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(diagramRoot, false);
        PlaceTopLeft((RectTransform)go.transform, x, y, w, h);
        go.GetComponent<Image>().color = CardBg;
        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor    = new Color(0f, 0f, 0f, 0.50f);
        shadow.effectDistance = new Vector2(5f, -6f);
        go.transform.localEulerAngles = new Vector3(0f, 0f, unityZRot);

        MakePinCompound(go.transform, StartPin);

        var lbl = MakeTMP(go.transform, "Label", "Start", 19.5f, CardInk, font);
        Stretch((RectTransform)lbl.transform);
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.fontStyle = FontStyles.UpperCase;
    }

    TextMeshProUGUI MakePinCompound(Transform cardRoot, Color headColor)
    {
        var container = MakeRect(cardRoot, "PinRoot");
        var cRt       = (RectTransform)container.transform;
        cRt.anchorMin        = cRt.anchorMax = new Vector2(0.5f, 1f);
        cRt.pivot            = new Vector2(0.5f, 0f);
        cRt.anchoredPosition = new Vector2(0f, -8f);
        cRt.sizeDelta        = new Vector2(26f, 34f);

        var stemGo  = new GameObject("Stem", typeof(RectTransform), typeof(Image));
        stemGo.transform.SetParent(container.transform, false);
        var stemImg = stemGo.GetComponent<Image>();
        stemImg.color         = PinStem;
        stemImg.raycastTarget = false;
        var stemRt  = (RectTransform)stemGo.transform;
        stemRt.anchorMin        = stemRt.anchorMax = new Vector2(0.5f, 0f);
        stemRt.pivot            = new Vector2(0.5f, 0f);
        stemRt.anchoredPosition = Vector2.zero;
        stemRt.sizeDelta        = new Vector2(4f, 12f);

        var head  = MakeTMP(container.transform, "Head", "●", 26f, headColor, null);
        var hRt   = (RectTransform)head.transform;
        hRt.anchorMin        = hRt.anchorMax = new Vector2(0.5f, 1f);
        hRt.pivot            = new Vector2(0.5f, 1f);
        hRt.anchoredPosition = Vector2.zero;
        hRt.sizeDelta        = new Vector2(30f, 30f);
        head.alignment       = TextAlignmentOptions.Center;
        var sh               = head.gameObject.AddComponent<Shadow>();
        sh.effectColor       = new Color(0f, 0f, 0f, 0.60f);
        sh.effectDistance    = new Vector2(2f, -2.5f);

        return head;
    }

    LineGroup MakeBezierLine(string groupName, Vector2 p0, Vector2 cp, Vector2 p1)
    {
        const int N = 10;

        var p0u = new Vector2( p0.x, -p0.y);
        var cpu = new Vector2( cp.x, -cp.y);
        var p1u = new Vector2( p1.x, -p1.y);

        var segs = new Image[N];
        for (int i = 0; i < N; i++)
        {
            Vector2 a = Bezier(p0u, cpu, p1u, (float)i       / N);
            Vector2 b = Bezier(p0u, cpu, p1u, (float)(i + 1) / N);
            Vector2 d = b - a;

            var go  = new GameObject($"{groupName}_{i}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(diagramRoot, false);
            var img = go.GetComponent<Image>();
            img.color = StringMuted;
            img.raycastTarget = false;

            var rt = (RectTransform)go.transform;
            rt.anchorMin        = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot            = new Vector2(0f, 0.5f);
            rt.anchoredPosition = a;
            rt.sizeDelta        = new Vector2(d.magnitude + 0.5f, 4.5f);
            rt.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);

            segs[i] = img;
        }
        return new LineGroup(segs);
    }

    static Vector2 Bezier(Vector2 p0, Vector2 cp, Vector2 p1, float t)
    {
        float mt = 1f - t;
        return mt * mt * p0 + 2f * mt * t * cp + t * t * p1;
    }

    static void PlaceTopLeft(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin        = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot            = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta        = new Vector2(w, h);
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static GameObject MakeRect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static TextMeshProUGUI MakeTMP(Transform parent, string name, string text,
                                    float size, Color color, TMP_FontAsset font)
    {
        var go  = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text          = text;
        tmp.fontSize      = size;
        tmp.color         = color;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;
        return tmp;
    }

    static TMP_FontAsset TryLoadFont(string path)
    {
        var f = Resources.Load<TMP_FontAsset>(path);
        if (f == null)
            Debug.LogWarning($"[CaseBoardUI] Font not found at Resources/{path}. " +
                             "Import a TMP SDF asset into Assets/TextMesh Pro/Resources/Fonts & Materials/.");
        return f;
    }

    struct LineGroup
    {
        Image[] _segs;
        public LineGroup(Image[] segs) => _segs = segs;

        public void Set(bool resolved, bool visible)
        {
            if (_segs == null) return;
            for (int i = 0; i < _segs.Length; i++)
            {
                if (_segs[i] == null) continue;
                bool show = visible && (resolved || i % 2 == 0);
                _segs[i].gameObject.SetActive(show);
                if (show) _segs[i].color = resolved ? StringRed : StringMuted;
            }
        }
    }

    class NodeCard
    {
        public RectTransform   root;
        public GameObject      pinRoot;
        public TextMeshProUGUI pin;
        public TextMeshProUGUI placeholder;
        public GameObject      content;
        public TextMeshProUGUI tick;
        public TextMeshProUGUI label;
        public TextMeshProUGUI value;
        public CanvasGroup     cg;

        public bool IsVisible => root != null && root.gameObject.activeSelf;

        public void SetActive(bool v) => root?.gameObject.SetActive(v);

        public void ShowPlaceholder()
        {
            root?.gameObject.SetActive(true);
            pinRoot?.SetActive(false);
            if (placeholder != null) placeholder.gameObject.SetActive(true);
            content?.SetActive(false);
        }

        public void ShowDeadEnd()
        {
            root?.gameObject.SetActive(true);
            pinRoot?.SetActive(true);
            if (pin != null) pin.color = PinGrayLight;
            if (placeholder != null) placeholder.gameObject.SetActive(true);
            content?.SetActive(false);
        }

        public void ShowNode(bool arrested, string name)
        {
            root?.gameObject.SetActive(true);
            if (placeholder != null) placeholder.gameObject.SetActive(false);
            content?.SetActive(true);

            pinRoot?.SetActive(true);
            if (pin != null) pin.color = arrested ? PinRedLight : PinBlueLight;
            if (tick  != null) tick.gameObject.SetActive(arrested);
            if (label != null) label.text = arrested ? "Arrested:" : "Suspect:";
            if (value != null) value.text = string.IsNullOrEmpty(name) ? "???" : name;
        }

        public void ShowMerged(bool resolved, string name)
        {
            ShowNode(resolved, name);
            if (label != null) label.text = "The Real Culprit";
        }
    }
}
