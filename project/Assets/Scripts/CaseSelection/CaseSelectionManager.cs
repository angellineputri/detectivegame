using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaseSelectionManager : MonoBehaviour
{
    [Header("Cases")]

    public CaseData[] cases;

    [Header("Tabs")]

    public Transform caseListParent;

    public GameObject caseEntryPrefab;

    [Header("Ticket Detail Panel")]

    public TextMeshProUGUI detailCaseName;

    public TextMeshProUGUI detailDescription;

    public TextMeshProUGUI objectivesText;

    public GameObject lockedNotice;

    [Header("Buttons")]

    public Button confirmButton;

    CaseData _selectedCase;
    readonly List<CaseEntryUI> _entries = new List<CaseEntryUI>();

    void Start()
    {
        confirmButton.interactable = false;
        if (lockedNotice != null)
            lockedNotice.SetActive(false);

        PopulateTabs();

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
        AudioManager.Instance?.PlayButtonClick();
        _selectedCase = caseData;

        foreach (CaseEntryUI entry in _entries)
            entry.SetSelected(entry.CaseData == caseData);

        if (detailCaseName != null)
            detailCaseName.text = caseData.caseName;

        if (detailDescription != null)
            detailDescription.text = caseData.description;

        if (objectivesText != null)
            objectivesText.text = caseData.description;

        if (lockedNotice != null)
            lockedNotice.SetActive(!caseData.isUnlocked);

        confirmButton.interactable = caseData.isUnlocked;
    }

    public void OnConfirmClicked()
    {
        if (_selectedCase == null || !_selectedCase.isUnlocked) return;
        if (!CheckGameManager()) return;

        AudioManager.Instance?.PlayButtonClick();
        GameManager.Instance.SelectCase(_selectedCase.caseID);
        GameManager.Instance.LoadScene(_selectedCase.startingScene);
    }

    public void OnBackClicked()
    {
        if (!CheckGameManager()) return;

        AudioManager.Instance?.PlayButtonClick();
        GameManager.Instance.LoadScene("MainMenu");
    }

    bool CheckGameManager()
    {
        if (GameManager.Instance != null) return true;
        return false;
    }
}
