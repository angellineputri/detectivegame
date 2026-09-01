// TAB PREFAB SETUP (create once in Assets/Prefabs/UI/CaseEntryTab.prefab):
//
//  Root  GameObject  — Button + Image (tabBackground) + CaseEntryUI + LayoutElement
//    │     LayoutElement: preferredHeight = 177  (fills TabStack evenly for 3 tabs + spacing)
//    │
//    └── Label  GameObject  — TextMeshProUGUI (caseNameLabel)
//          RectTransform: anchor center, pivot center, rotation Z = 90
//          Size: width = 150 (spans tab height), height = 110 (spans tab width)
//          Alignment: Center
//
//  Optional child:
//    └── LockedOverlay — Image (semi-transparent dark, alpha ~180)
//          Stretch-fill the root; SetActive(false) by default
//          ART FLAG: swap for a "?" or "LOCKED" graphic when art is ready
//
// Wire in Inspector: Tab Background = root Image, Case Name Label = Label TMP,
//   Select Button = root Button, Locked Overlay = LockedOverlay (or leave null).

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaseEntryUI : MonoBehaviour
{
    [Header("Tab Visuals")]
    public Image tabBackground;
    // ART FLAG: these are placeholder colors — swap in real sprites when art is ready.
    [SerializeField] Color selectedColor = new Color(0.722f, 0.361f, 0.282f); // terracotta
    [SerializeField] Color defaultColor  = Color.white;

    [Header("References")]
    public TextMeshProUGUI caseNameLabel;
    public Button selectButton;
    // Semi-transparent overlay shown on top of the tab when the case is locked.
    public GameObject lockedOverlay;

    public CaseData CaseData { get; private set; }

    Action<CaseData> _onSelected;

    public void Init(CaseData data, Action<CaseData> onSelected)
    {
        CaseData = data;
        _onSelected = onSelected;
        caseNameLabel.text = data.caseName;
        if (lockedOverlay != null)
            lockedOverlay.SetActive(!data.isUnlocked);
        SetSelected(false);
        selectButton.onClick.AddListener(OnClicked);
    }

    public void SetSelected(bool isSelected)
    {
        if (tabBackground != null)
            tabBackground.color = isSelected ? selectedColor : defaultColor;
    }

    void OnClicked()
    {
        _onSelected?.Invoke(CaseData);
    }
}
