using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Interactable : MonoBehaviour
{
    [Header("Identity")]

    [Tooltip("Unique ID used by GameManager to track this clue.")]
    public string objectID;

    [Tooltip("Short name shown in the evidence bag. Keep under ~20 chars.")]
    public string displayName;

    [Tooltip("What the player first observes. Shown immediately on interaction.")]
    [TextArea(2, 4)]
    public string noticeText;

    [Tooltip("What they find on closer inspection. Clue is added to bag only after this is dismissed.")]
    [TextArea(2, 4)]
    public string revealText;

    [Tooltip("Whether this clue appears in the evidence bag.")]
    public bool addToBag = true;

    [Header("Gating")]

    [Tooltip("0 = visible in any playthrough. Set to 1 or 2 to restrict.")]
    public int requiredPlaythrough = 0;

    [Tooltip("GameManager flag that must be true for this object to be active. Leave empty to skip.")]
    public string requiredFlag;

    [Tooltip("Clue ID that the player must have before this object can be examined. Leave empty if no clue is required.")]
    public string requiredClue;

    [Tooltip("Maximum distance from the player required to interact.")]
    public float interactRange = 2f;

    [Header("Visuals")]

    [Tooltip("Optional child GameObject to hide when this object is inactive.")]
    [SerializeField] GameObject visualRoot;

    [NonSerialized] public bool interactionLocked;

    public static bool GlobalHighlightOverride;

    public static bool GlobalInteractionLocked;

    SpriteRenderer _spriteRenderer;
    Color _originalColor;
    bool _isHighlighted;

    protected virtual void Start()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }

        RefreshActiveState();

        if (addToBag && !string.IsNullOrEmpty(objectID))
        {
            BagUI.Instance?.RegisterClueDisplayName(
                objectID,
                displayName
            );
        }
    }

    void Update()
    {
        if (_spriteRenderer == null) return;

        bool playerInControl = GlobalHighlightOverride ||
                               PlayerController.Instance == null ||
                               PlayerController.Instance.CanMove;
        bool shouldHighlight = playerInControl && !interactionLocked && IsActiveThisPlaythrough() && InRange();

        if (shouldHighlight && !_isHighlighted)
        {
            _spriteRenderer.color = Color.Lerp(_originalColor, Color.red, 0.6f);
            _isHighlighted = true;
            Debug.Log("Highlight ON: " + gameObject.name);
        }
        else if (!shouldHighlight && _isHighlighted)
        {
            _spriteRenderer.color = _originalColor;
            _isHighlighted = false;
            Debug.Log("Highlight OFF: " + gameObject.name);
        }
    }

    public virtual void RefreshActiveState()
    {
        bool active = IsActiveThisPlaythrough();

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = active;
        }

        if (visualRoot != null)
        {
            visualRoot.SetActive(active);
        }

        if (!active && _isHighlighted && _spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
            _isHighlighted = false;
        }
    }

    public bool IsActiveThisPlaythrough()
    {
        if (GameManager.Instance == null)
        {
            return true;
        }

        bool playthroughOk =
            requiredPlaythrough == 0 ||
            requiredPlaythrough == GameManager.Instance.CurrentPlaythrough;

        bool flagOk =
            string.IsNullOrEmpty(requiredFlag) ||
            GameManager.Instance.GetFlag(requiredFlag);

        return playthroughOk && flagOk;
    }

    protected bool HasRequiredClue()
    {
        if (string.IsNullOrEmpty(requiredClue))
        {
            return true;
        }

        if (GameManager.Instance == null)
        {
            return false;
        }

        return GameManager.Instance.HasClue(requiredClue);
    }

    protected void ShowMissingClueMessage()
    {
        UIManager.Instance?.ShowCluePopup(
            "I need more information before I can examine this.",
            ""
        );
    }

    protected virtual void OnMouseDown()
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (DialogueRunner.Instance != null &&
            DialogueRunner.Instance.IsPlaying)
        {
            return;
        }

        if (UIManager.Instance != null &&
            UIManager.Instance.IsPopupVisible)
        {
            return;
        }

        if (interactionLocked)
        {
            return;
        }

        if (!IsActiveThisPlaythrough())
        {
            return;
        }

        if (!HasRequiredClue())
        {
            ShowMissingClueMessage();
            return;
        }

        if (!InRange())
        {
            return;
        }

        OnInteract();
    }

    public void TriggerInteract()
    {
        if (interactionLocked)
        {
            return;
        }

        if (!IsActiveThisPlaythrough())
        {
            return;
        }

        if (!HasRequiredClue())
        {
            ShowMissingClueMessage();
            return;
        }

        OnInteract();
    }

    protected bool InRange()
    {
        if (PlayerController.Instance == null)
        {
            return true;
        }

        float dist = Vector2.Distance(
            transform.position,
            PlayerController.Instance.transform.position
        );

        return dist <= interactRange;
    }

    protected virtual void OnInteract()
    {
        if (!string.IsNullOrEmpty(noticeText))
        {
            UIManager.Instance?.ShowCluePopup(
                noticeText,
                revealText,
                OnRevealComplete
            );
        }
    }

    protected void OnRevealComplete()
    {
        if (!string.IsNullOrEmpty(objectID))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddClue(objectID);
            }
        }
    }
}
