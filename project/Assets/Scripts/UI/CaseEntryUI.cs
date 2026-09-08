using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaseEntryUI : MonoBehaviour
{
    [Header("Tab Visuals")]
    public Image tabBackground;

    [SerializeField] Color selectedColor = new Color(0.722f, 0.361f, 0.282f);
    [SerializeField] Color defaultColor  = Color.white;

    [Header("References")]
    public TextMeshProUGUI caseNameLabel;
    public Button selectButton;

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
