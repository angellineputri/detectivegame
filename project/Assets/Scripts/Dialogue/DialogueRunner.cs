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
    bool _dimOverlayEnabled = true;
    bool _keyboardAdvanceDisabled;
    Sprite _currentPortrait;

    public void SetDimOverlayEnabled(bool enabled) => _dimOverlayEnabled = enabled;

    public void SetKeyboardAdvanceDisabled(bool disabled) => _keyboardAdvanceDisabled = disabled;

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
            dialogueDimOverlay.transform.SetAsLastSibling();
        }

        dialoguePanel.transform.SetAsLastSibling();
        advanceButton?.onClick.AddListener(Advance);

        TMP_FontAsset ps2p = Resources.Load<TMP_FontAsset>("Fonts & Materials/PressStart2P SDF");
        TMP_FontAsset vt323 = Resources.Load<TMP_FontAsset>("Fonts & Materials/VT323 SDF");
        if (ps2p != null && speakerText != null)
        {
            speakerText.font = ps2p;
            speakerText.fontSize = 22;
            RectTransform speakerRT = speakerText.GetComponent<RectTransform>();
            if (speakerRT != null)
            {
                Vector2 sd = speakerRT.sizeDelta;
                speakerRT.sizeDelta = new Vector2(600f, sd.y);
            }
        }
        if (vt323 != null && bodyText != null) { bodyText.font = vt323; bodyText.fontSize = 30; }
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

        if (!_keyboardAdvanceDisabled &&
            (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)))
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
            dialogueDimOverlay.SetActive(_dimOverlayEnabled);
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
            CloseDialogue();
        }
    }

    public void ForceComplete()
    {
        if (!IsPlaying) return;
        CloseDialogue();
    }

    void CloseDialogue()
    {
        if (dialogueDimOverlay != null)
            dialogueDimOverlay.SetActive(false);
        _dimOverlayEnabled = true;
        _keyboardAdvanceDisabled = false;
        dialoguePanel.SetActive(false);
        if (BagUI.Instance != null)
            BagUI.Instance.RefreshBagHudVisibility();
        _justClosed = true;
        _onComplete?.Invoke();
    }
}
