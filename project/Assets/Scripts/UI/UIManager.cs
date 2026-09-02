using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Clue Popup")]
    [SerializeField] GameObject cluePanel;
    [SerializeField] TMP_Text clueText;
    [SerializeField] Button closeButton;
    [SerializeField] GameObject clueDimOverlay;

    string _revealText;
    System.Action _onComplete;
    bool _showingReveal;
    bool _justOpened;
    bool _justClosed;

    public bool WasJustClosed
    {
        get { return _justClosed; }
    }

    public bool IsPopupVisible
    {
        get { return cluePanel != null && cluePanel.activeSelf; }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        cluePanel.SetActive(false);
        if (clueDimOverlay != null)
        {
            clueDimOverlay.SetActive(false);
        }
        closeButton?.onClick.AddListener(HideCluePopup);
    }

    void Update()
    {
        _justClosed = false;

        if (!cluePanel.activeSelf) return;

        if (_justOpened)
        {
            _justOpened = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
        {
            Advance();
        }
    }

    public void ShowCluePopup(string noticeText, string revealText, System.Action onComplete = null)
    {
        clueText.text = noticeText;
        _revealText = revealText;
        _onComplete = onComplete;
        _showingReveal = false;
        _justOpened = true;
        if (clueDimOverlay != null)
        {
            clueDimOverlay.SetActive(true);
        }
        cluePanel.SetActive(true);
        if (BagUI.Instance != null)
        {
            BagUI.Instance.RefreshBagHudVisibility();
        }
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }
    }

    void Advance()
    {
        if (_showingReveal || string.IsNullOrEmpty(_revealText))
        {
            System.Action cb = _onComplete;
            _onComplete = null;
            HideCluePopup();
            _justClosed = true;
            cb?.Invoke();
        }
        else
        {
            _showingReveal = true;
            clueText.text = _revealText;
            _justOpened = true;
        }
    }

    public void HideCluePopup()
    {
        if (clueDimOverlay != null)
        {
            clueDimOverlay.SetActive(false);
        }
        cluePanel.SetActive(false);
        if (BagUI.Instance != null)
        {
            BagUI.Instance.RefreshBagHudVisibility();
        }
        _showingReveal = false;
        _onComplete = null;
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = true;
        }
    }
}
