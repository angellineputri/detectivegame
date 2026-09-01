using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Scene-specific controller — attach to a single empty GameObject in SelectCaseScreen.unity.
// Not persistent (no DontDestroyOnLoad); relies on GameManager for state handoff.
public class CaseSelectionManager : MonoBehaviour
{
    [Header("Cases")]
    // Drag your CaseData ScriptableObject assets here, one per tab, in display order.
    public CaseData[] cases;

    [Header("Tabs")]
    // The TabStack RectTransform — parent for instantiated CaseEntryUI tab prefabs.
    public Transform caseListParent;
    // Prefab with a CaseEntryUI component, a rotated TMP label, and a Button.
    public GameObject caseEntryPrefab;

    [Header("Ticket Detail Panel")]
    // Optional: a TMP label that updates with the selected case name (e.g. a subtitle below the title).
    // Can be left unassigned if there is no dedicated case-name label in the ticket.
    public TextMeshProUGUI detailCaseName;
    // InfoText TMP inside the left column — shows CaseData.description.
    public TextMeshProUGUI detailDescription;
    // ObjectivesText TMP inside the right column — shows objectives placeholder.
    public TextMeshProUGUI objectivesText;
    // "Coming Soon" or locked notice object inside the ticket, hidden when unlocked case is selected.
    public GameObject lockedNotice;

    [Header("Buttons")]
    // Wire OnConfirmClicked() and OnBackClicked() via Inspector onClick events.
    public Button confirmButton;

    CaseData _selectedCase;
    readonly List<CaseEntryUI> _entries = new List<CaseEntryUI>();

    void Start()
    {
        confirmButton.interactable = false;
        if (lockedNotice != null)
            lockedNotice.SetActive(false);

        PopulateTabs();

        // Auto-select the first unlocked case so the ticket isn't blank on open.
        foreach (CaseData c in cases)
        {
            if (c.isUnlocked)
            {
                OnCaseSelected(c);
                break;
            }
        }
    }

    void PopulateTabs()
    {
        foreach (CaseData caseData in cases)
        {
            GameObject entry = Instantiate(caseEntryPrefab, caseListParent);
            CaseEntryUI entryUI = entry.GetComponent<CaseEntryUI>();
            _entries.Add(entryUI);
            entryUI.Init(caseData, OnCaseSelected);
        }
    }

    void OnCaseSelected(CaseData caseData)
    {
        _selectedCase = caseData;

        foreach (CaseEntryUI entry in _entries)
            entry.SetSelected(entry.CaseData == caseData);

        if (detailCaseName != null)
            detailCaseName.text = caseData.caseName;

        if (detailDescription != null)
            detailDescription.text = caseData.description;

        if (objectivesText != null)
            objectivesText.text = caseData.description; // placeholder — replace with a dedicated field later

        if (lockedNotice != null)
            lockedNotice.SetActive(!caseData.isUnlocked);

        confirmButton.interactable = caseData.isUnlocked;
    }

    public void OnConfirmClicked()
    {
        if (_selectedCase == null || !_selectedCase.isUnlocked) return;
        if (!CheckGameManager()) return;

        GameManager.Instance.SelectCase(_selectedCase.caseID);
        GameManager.Instance.LoadScene(_selectedCase.startingScene);
    }

    public void OnBackClicked()
    {
        if (!CheckGameManager()) return;

        GameManager.Instance.LoadScene("MainMenu");
    }

    bool CheckGameManager()
    {
        if (GameManager.Instance != null) return true;
        Debug.LogWarning("[CaseSelectionManager] GameManager.Instance is null — scene load skipped. " +
                         "Make sure a GameManager GameObject exists in the scene or was loaded from MainMenu.");
        return false;
    }
}
