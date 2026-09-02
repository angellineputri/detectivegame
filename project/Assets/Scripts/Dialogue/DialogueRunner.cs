using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueRunner : MonoBehaviour
{
    public static DialogueRunner Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] GameObject dialoguePanel;
    [SerializeField] TMP_Text speakerText;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] Button advanceButton;
    [SerializeField] GameObject dialogueDimOverlay;
    [SerializeField] Image speakerPortraitImage;

    DialogueLine[] _lines;
    int _index;
    Action _onComplete;
    bool _justOpened;
    bool _justClosed;
    Sprite _currentPortrait;

    public bool IsPlaying
    {
        get { return dialoguePanel != null && dialoguePanel.activeSelf; }
    }

    public bool WasJustClosed
    {
        get { return _justClosed; }
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

        dialoguePanel.SetActive(false);
        if (dialogueDimOverlay != null)
        {
            dialogueDimOverlay.SetActive(false);
        }
        advanceButton?.onClick.AddListener(Advance);
    }

    void Update()
    {
        _justClosed = false;

        if (!dialoguePanel.activeSelf) return;

        if (_justOpened)
        {
            _justOpened = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            Advance();
        }
    }

    public void Play(DialogueData data, Action onComplete = null)
    {
        if (data == null || data.lines.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        _lines = data.lines;
        _index = 0;
        _onComplete = onComplete;
        _justOpened = true;
        if (dialogueDimOverlay != null)
        {
            dialogueDimOverlay.SetActive(true);
        }
        dialoguePanel.SetActive(true);
        if (BagUI.Instance != null)
        {
            BagUI.Instance.RefreshBagHudVisibility();
        }
        ShowLine();
    }

    void ShowLine()
    {
        DialogueLine line = _lines[_index];
        speakerText.text = line.speaker;
        bodyText.text = line.text;
        if (speakerPortraitImage != null)
        {
            if (line.speakerPortrait != null)
            {
                speakerPortraitImage.sprite = line.speakerPortrait;
                speakerPortraitImage.gameObject.SetActive(true);
            }
            else
            {
                speakerPortraitImage.gameObject.SetActive(false);
            }
        }
    }

    void Advance()
    {
        _index++;
        if (_index < _lines.Length)
        {
            ShowLine();
        }
        else
        {
            if (dialogueDimOverlay != null)
            {
                dialogueDimOverlay.SetActive(false);
            }
            dialoguePanel.SetActive(false);
            if (BagUI.Instance != null)
            {
                BagUI.Instance.RefreshBagHudVisibility();
            }
            _justClosed = true;
            _onComplete?.Invoke();
        }
    }
}
