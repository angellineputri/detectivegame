using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BagUI : MonoBehaviour
{
    public static BagUI Instance { get; private set; }

    [Header("Bag HUD")]
    [SerializeField] GameObject bagHud;
    [SerializeField] Button bagButton;
    [SerializeField] TMP_Text badgeText;
    [SerializeField] Button makeAMoveButton;
    [SerializeField] string makeAMoveTitle = "Where to next?";

    [Header("Review Panel")]
    [SerializeField] GameObject reviewPanel;
    [SerializeField] GameObject bagReviewDimOverlay;
    [SerializeField] TMP_Text panelTitleText;
    [SerializeField] RectTransform clueGridRoot;
    [SerializeField] Button closeButton;
    [SerializeField] Button keepExploringButton;

    [Header("Cell Font")]
    [SerializeField] TMP_FontAsset vt323;

    static readonly Color32 CellBg = new Color32(0xf4, 0xf5, 0xf6, 0xff);
    static readonly Color32 CellInk = new Color32(0x24, 0x2a, 0x30, 0xff);
    static readonly Color32 BorderColor = new Color32(0x0e, 0x11, 0x14, 0xff);
    const float CellHeight = 68f;
    const float BorderWidth = 4f;

    Dictionary<string, string> _displayNames = new Dictionary<string, string>();
    HashSet<string> _spawnedClueIDs = new HashSet<string>();
    List<GameObject> _spawnedRows = new List<GameObject>();
    List<Button> _spawnedButtons = new List<Button>();
    int _itemCount;

    ClueOutcomeTable _currentTable;
    System.Action _onKeepExploring;
    bool _forcedDecisionMode;
    bool _hudPinned;
    bool _tutorialCloseOnly;
    string _defaultTitle;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        TMP_FontAsset ps2p = Resources.Load<TMP_FontAsset>("Fonts & Materials/PressStart2P SDF");
        if (ps2p != null && badgeText != null) { badgeText.font = ps2p; badgeText.fontSize = 14; }

        SetHudVisible(false);
        if (reviewPanel != null) reviewPanel.SetActive(false);
        if (bagReviewDimOverlay != null) bagReviewDimOverlay.SetActive(false);

        bagButton?.onClick.AddListener(OnBagButtonClicked);
        closeButton?.onClick.AddListener(CloseReviewPanel);
        keepExploringButton?.onClick.AddListener(OnKeepExploringClicked);
        makeAMoveButton?.onClick.AddListener(OnMakeAMoveClicked);

        if (keepExploringButton != null) keepExploringButton.gameObject.SetActive(false);

        _defaultTitle = panelTitleText != null ? panelTitleText.text : "";

        UpdateBadge();
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClueAdded += HandleClueAdded;
            GameManager.Instance.OnPlaythroughAdvanced += ClearBag;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClueAdded -= HandleClueAdded;
            GameManager.Instance.OnPlaythroughAdvanced -= ClearBag;
        }
    }

    public void RegisterClueDisplayName(string clueID, string name)
    {
        _displayNames[clueID] = string.IsNullOrEmpty(name) ? clueID : name;
    }

    public string GetDisplayName(string clueID)
    {
        string name;
        return _displayNames.TryGetValue(clueID, out name) ? name : clueID;
    }

    public void SetOutcomeTable(ClueOutcomeTable table) => _currentTable = table;

    public bool IsReviewPanelOpen => reviewPanel != null && reviewPanel.activeSelf;

    public void EnterTutorialCloseOnly()
    {
        _tutorialCloseOnly = true;
        if (makeAMoveButton != null) makeAMoveButton.gameObject.SetActive(false);
    }

    public void ExitTutorialCloseOnly()
    {
        _tutorialCloseOnly = false;
        if (!_forcedDecisionMode && makeAMoveButton != null)
            makeAMoveButton.gameObject.SetActive(true);
    }

    public void SetHudPinned(bool pinned)
    {
        _hudPinned = pinned;
        if (pinned) SetHudVisible(true);
    }

    public void SetHudVisible(bool visible)
    {
        if (bagHud != null) bagHud.SetActive(visible);
        if (visible && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        if (!visible)
        {
            if (reviewPanel != null) reviewPanel.SetActive(false);
            if (bagReviewDimOverlay != null) bagReviewDimOverlay.SetActive(false);
        }
    }

    public void RefreshBagHudVisibility()
    {
        if (_hudPinned) return;
        bool anyOpen = (reviewPanel != null && reviewPanel.activeSelf)
            || (DialogueRunner.Instance != null && DialogueRunner.Instance.IsPlaying)
            || (UIManager.Instance != null && UIManager.Instance.IsPopupVisible);
        if (bagHud != null) bagHud.SetActive(!anyOpen);
    }

    public void ForceOpenForDecision(string title, System.Action onKeepExploring)
    {
        _forcedDecisionMode = true;
        _onKeepExploring = onKeepExploring;

        if (panelTitleText != null) panelTitleText.text = title;

        closeButton?.gameObject.SetActive(false);
        keepExploringButton?.gameObject.SetActive(true);
        makeAMoveButton?.gameObject.SetActive(false);

        foreach (Button b in _spawnedButtons) b.interactable = true;
        if (bagReviewDimOverlay != null) bagReviewDimOverlay.SetActive(true);
        if (reviewPanel != null) reviewPanel.SetActive(true);
        if (clueGridRoot != null) LayoutRebuilder.ForceRebuildLayoutImmediate(clueGridRoot);
        RefreshBagHudVisibility();

        if (PlayerController.Instance != null) PlayerController.Instance.CanMove = false;
    }

    void ExitForcedDecision()
    {
        _forcedDecisionMode = false;
        _onKeepExploring = null;

        if (panelTitleText != null) panelTitleText.text = _defaultTitle;

        foreach (Button b in _spawnedButtons) b.interactable = false;
        closeButton?.gameObject.SetActive(true);
        keepExploringButton?.gameObject.SetActive(false);
        if (!_tutorialCloseOnly)
            makeAMoveButton?.gameObject.SetActive(true);
    }

    void OnBagButtonClicked()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) return;
        if (_forcedDecisionMode) return;

        if (DialogueRunner.Instance != null && DialogueRunner.Instance.IsPlaying)
        {
            if (_hudPinned) DialogueRunner.Instance.ForceComplete();
            else return;
        }

        if (UIManager.Instance != null && UIManager.Instance.IsPopupVisible) return;

        bool newState = reviewPanel != null && !reviewPanel.activeSelf;
        if (bagReviewDimOverlay != null) bagReviewDimOverlay.SetActive(newState);
        if (reviewPanel != null) reviewPanel.SetActive(newState);
        if (newState && clueGridRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(clueGridRoot);
        RefreshBagHudVisibility();
    }

    void CloseReviewPanel()
    {
        if (_hudPinned && DialogueRunner.Instance != null && DialogueRunner.Instance.IsPlaying)
            DialogueRunner.Instance.ForceComplete();

        if (bagReviewDimOverlay != null) bagReviewDimOverlay.SetActive(false);
        if (reviewPanel != null) reviewPanel.SetActive(false);
        RefreshBagHudVisibility();
    }

    void OnMakeAMoveClicked()
    {
        ForceOpenForDecision(makeAMoveTitle, null);
    }

    void OnKeepExploringClicked()
    {
        System.Action cb = _onKeepExploring;
        ExitForcedDecision();
        CloseReviewPanel();
        if (PlayerController.Instance != null) PlayerController.Instance.CanMove = true;
        cb?.Invoke();
    }

    void HandleClueAdded(string clueID)
    {
        if (_spawnedClueIDs.Contains(clueID)) return;
        if (!_displayNames.ContainsKey(clueID))
            _displayNames[clueID] = clueID;

        _spawnedClueIDs.Add(clueID);
        SpawnEntry(clueID, _displayNames[clueID]);
        UpdateBadge();
    }

    public void ClearBag()
    {
        foreach (GameObject row in _spawnedRows)
            if (row != null) Destroy(row);
        _spawnedRows.Clear();
        _spawnedButtons.Clear();
        _spawnedClueIDs.Clear();
        _displayNames.Clear();
        _itemCount = 0;
        CloseReviewPanel();
        UpdateBadge();
    }

    void UpdateBadge()
    {
        if (badgeText != null)
            badgeText.text = _spawnedClueIDs.Count > 0 ? _spawnedClueIDs.Count.ToString() : "";
    }

    void SpawnEntry(string clueID, string displayName)
    {
        if (clueGridRoot == null) return;

        GameObject row;
        if (_itemCount % 2 == 0)
        {
            row = new GameObject("Row", typeof(RectTransform));
            row.transform.SetParent(clueGridRoot, false);

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.minHeight = CellHeight;
            le.preferredHeight = CellHeight;

            _spawnedRows.Add(row);
        }
        else
        {
            row = _spawnedRows[_spawnedRows.Count - 1];
        }

        MakeCell(row.transform, clueID, displayName);
        _itemCount++;
    }

    void MakeCell(Transform parent, string clueID, string displayName)
    {
        GameObject outer = new GameObject("Cell", typeof(RectTransform), typeof(Image));
        outer.transform.SetParent(parent, false);
        outer.GetComponent<Image>().color = BorderColor;

        GameObject inner = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        inner.transform.SetParent(outer.transform, false);
        inner.GetComponent<Image>().color = CellBg;
        RectTransform innerRT = inner.GetComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero;
        innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = new Vector2(BorderWidth, BorderWidth);
        innerRT.offsetMax = new Vector2(-BorderWidth, -BorderWidth);

        GameObject labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(inner.transform, false);
        TextMeshProUGUI tmp = labelGO.GetComponent<TextMeshProUGUI>();
        tmp.text = displayName;
        tmp.fontSize = 22;
        tmp.color = CellInk;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        if (vt323 != null) tmp.font = vt323;
        RectTransform textRT = labelGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(18f, 12f);
        textRT.offsetMax = new Vector2(-18f, -12f);

        Button btn = outer.AddComponent<Button>();
        btn.targetGraphic = inner.GetComponent<Image>();
        btn.interactable = false;
        ColorBlock cb = ColorBlock.defaultColorBlock;
        cb.normalColor = CellBg;
        cb.highlightedColor = new Color32(0xd0, 0xd3, 0xd6, 0xff);
        cb.pressedColor = new Color32(0xb8, 0xbb, 0xbe, 0xff);
        cb.disabledColor = CellBg;
        cb.fadeDuration = 0.05f;
        btn.colors = cb;
        btn.onClick.AddListener(() => OnClueEntryClicked(clueID));
        _spawnedButtons.Add(btn);
    }

    void OnClueEntryClicked(string clueID)
    {
        if (!_forcedDecisionMode) return;

        if (_currentTable == null)
        {
            Debug.LogWarning("[BagUI] No outcome table set for this scene.");
            ExitForcedDecision();
            return;
        }

        ClueOutcome outcome = _currentTable.GetOutcome(clueID);
        if (outcome == null)
        {
            CloseReviewPanel();
            ExitForcedDecision();
            GameOverScreen.Instance?.Show();
            return;
        }

        CloseReviewPanel();
        ExitForcedDecision();
        ItemSelectionUI.Instance?.ExecuteOutcome(outcome);
    }
}
