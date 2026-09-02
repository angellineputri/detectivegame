using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] Transform clueListContainer;
    [SerializeField] GameObject clueEntryPrefab;
    [SerializeField] Button closeButton;
    [SerializeField] Button keepExploringButton;

    Dictionary<string, string> _displayNames = new Dictionary<string, string>();
    HashSet<string> _spawnedClueIDs = new HashSet<string>();
    List<GameObject> _spawnedEntries = new List<GameObject>();

    ClueOutcomeTable _currentTable;

    System.Action _onKeepExploring;
    bool _forcedDecisionMode;
    string _defaultTitle;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetHudVisible(false);
        reviewPanel.SetActive(false);
        if (bagReviewDimOverlay != null)
        {
            bagReviewDimOverlay.SetActive(false);
        }
        bagButton.onClick.AddListener(OnBagButtonClicked);
        closeButton.onClick.AddListener(CloseReviewPanel);

        if (panelTitleText != null)
        {
            _defaultTitle = panelTitleText.text;
        }
        else
        {
            _defaultTitle = "";
        }

        if (keepExploringButton != null)
        {
            keepExploringButton.onClick.AddListener(OnKeepExploringClicked);
            keepExploringButton.gameObject.SetActive(false);
        }

        if (makeAMoveButton != null)
        {
            makeAMoveButton.onClick.AddListener(OnMakeAMoveClicked);
        }

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
        if (string.IsNullOrEmpty(name))
        {
            _displayNames[clueID] = clueID;
        }
        else
        {
            _displayNames[clueID] = name;
        }
    }

    public string GetDisplayName(string clueID)
    {
        string name;
        if (_displayNames.TryGetValue(clueID, out name))
        {
            return name;
        }
        return clueID;
    }

    public void SetOutcomeTable(ClueOutcomeTable table)
    {
        _currentTable = table;
    }

    public void SetHudVisible(bool visible)
    {
        if (bagHud != null)
        {
            bagHud.SetActive(visible);
        }

        if (!visible)
        {
            if (reviewPanel != null)
            {
                reviewPanel.SetActive(false);
            }
            if (bagReviewDimOverlay != null)
            {
                bagReviewDimOverlay.SetActive(false);
            }
        }
    }

    public void RefreshBagHudVisibility()
    {
        bool anyOpen = reviewPanel.activeSelf
            || (DialogueRunner.Instance != null && DialogueRunner.Instance.IsPlaying)
            || (UIManager.Instance != null && UIManager.Instance.IsPopupVisible);
        if (bagHud != null)
        {
            bagHud.SetActive(!anyOpen);
        }
    }

    public void ForceOpenForDecision(string title, System.Action onKeepExploring)
    {
        _forcedDecisionMode = true;
        _onKeepExploring = onKeepExploring;

        if (panelTitleText != null)
        {
            panelTitleText.text = title;
        }

        closeButton.gameObject.SetActive(false);

        if (keepExploringButton != null)
        {
            keepExploringButton.gameObject.SetActive(true);
        }

        if (makeAMoveButton != null)
        {
            makeAMoveButton.gameObject.SetActive(false);
        }

        if (bagReviewDimOverlay != null)
        {
            bagReviewDimOverlay.SetActive(true);
        }
        reviewPanel.SetActive(true);
        RefreshBagHudVisibility();

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }
    }

    void ExitForcedDecision()
    {
        _forcedDecisionMode = false;
        _onKeepExploring = null;

        if (panelTitleText != null)
        {
            panelTitleText.text = _defaultTitle;
        }

        closeButton.gameObject.SetActive(true);

        if (keepExploringButton != null)
        {
            keepExploringButton.gameObject.SetActive(false);
        }

        if (makeAMoveButton != null)
        {
            makeAMoveButton.gameObject.SetActive(true);
        }
    }

    void HandleClueAdded(string clueID)
    {
        if (!_displayNames.ContainsKey(clueID))
        {
            return;
        }

        if (_spawnedClueIDs.Contains(clueID))
        {
            return;
        }

        string displayName = _displayNames[clueID];
        _spawnedClueIDs.Add(clueID);
        SpawnEntry(clueID, displayName);
        UpdateBadge();
    }

    void OnBagButtonClicked()
    {
        if (_forcedDecisionMode)
        {
            return;
        }

        if (DialogueRunner.Instance != null && DialogueRunner.Instance.IsPlaying)
        {
            return;
        }

        if (UIManager.Instance != null && UIManager.Instance.IsPopupVisible)
        {
            return;
        }

        bool newState = !reviewPanel.activeSelf;
        if (bagReviewDimOverlay != null)
        {
            bagReviewDimOverlay.SetActive(newState);
        }
        reviewPanel.SetActive(newState);
        RefreshBagHudVisibility();
    }

    void CloseReviewPanel()
    {
        if (bagReviewDimOverlay != null)
        {
            bagReviewDimOverlay.SetActive(false);
        }
        reviewPanel.SetActive(false);
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

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = true;
        }

        if (cb != null)
        {
            cb();
        }
    }

    void SpawnEntry(string clueID, string displayName)
    {
        GameObject go = Instantiate(clueEntryPrefab, clueListContainer);
        _spawnedEntries.Add(go);

        go.GetComponentInChildren<TMP_Text>().text = displayName;
        go.GetComponent<Button>().onClick.AddListener(() => OnClueEntryClicked(clueID));
    }

    void OnClueEntryClicked(string clueID)
    {
        if (!_forcedDecisionMode)
        {
            return;
        }

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

            if (GameOverScreen.Instance != null)
            {
                GameOverScreen.Instance.Show();
            }

            return;
        }

        CloseReviewPanel();
        ExitForcedDecision();

        if (ItemSelectionUI.Instance != null)
        {
            ItemSelectionUI.Instance.ExecuteOutcome(outcome);
        }
    }

    public void ClearBag()
    {
        foreach (GameObject go in _spawnedEntries)
        {
            Destroy(go);
        }

        _spawnedEntries.Clear();
        _spawnedClueIDs.Clear();
        _displayNames.Clear();
        UpdateBadge();
    }

    void UpdateBadge()
    {
        if (badgeText != null)
        {
            if (_spawnedClueIDs.Count > 0)
            {
                badgeText.text = _spawnedClueIDs.Count.ToString();
            }
            else
            {
                badgeText.text = "";
            }
        }
    }
}
