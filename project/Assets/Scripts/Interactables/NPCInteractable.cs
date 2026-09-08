using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class ItemGrant
{
    public string objectID;
    public string displayName;

    [TextArea(2, 4)]
    public string clueDescription;
}

public class NPCInteractable : Interactable
{
    [Header("NPC Dialogue")]

    [Tooltip("Dialogue used when the required clue has NOT been found.")]
    public DialogueData dialogueBeforeRequirement;

    [Tooltip("Dialogue used when the required clue HAS been found.")]
    public DialogueData dialogueAfterRequirement;

    [Header("Item Grant Requirement")]

    [Tooltip("The clue the player must have before this NPC can give the reward. Leave empty to always allow.")]
    public string requiredClueToGrant;

    [Header("On Dialogue Complete")]

    [Tooltip("Flag to set after the NPC gives its reward. Leave empty to skip.")]
    public string flagToSetOnComplete;

    [Tooltip("Item/clue given to the player after the required clue has been found.")]
    public ItemGrant itemToGrantOnComplete;

    [Tooltip("Scene to load after the NPC interaction is completed. Leave empty to stay in the current scene.")]
    public string sceneToLoadOnComplete;

    [Tooltip("If true, the NPC GameObject is disabled after giving the reward.")]
    public bool deactivateSelfOnComplete;

    protected override void Start()
    {
        addToBag = false;

        base.Start();
    }

    public override void RefreshActiveState()
    {
        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
        }
    }

    protected override void OnMouseDown()
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

        if (!IsActiveThisPlaythrough())
        {
            return;
        }

        if (!InRange())
        {
            return;
        }

        OnInteract();
    }

    protected override void OnInteract()
    {
        if (HasCompletedReward())
        {
            return;
        }

        bool hasRequiredClue = HasRequiredClueForNPC();

        DialogueData selectedDialogue;

        if (hasRequiredClue)
        {
            selectedDialogue = dialogueAfterRequirement;
        }
        else
        {
            selectedDialogue = dialogueBeforeRequirement;
        }

        PlayDialogue(
            selectedDialogue,
            hasRequiredClue
        );
    }

    private bool HasRequiredClueForNPC()
    {
        if (string.IsNullOrEmpty(requiredClueToGrant))
        {
            return true;
        }

        if (GameManager.Instance == null)
        {
            return false;
        }

        return GameManager.Instance.HasClue(
            requiredClueToGrant
        );
    }

    private bool HasCompletedReward()
    {
        if (string.IsNullOrEmpty(flagToSetOnComplete))
        {
            return false;
        }

        if (GameManager.Instance == null)
        {
            return false;
        }

        return GameManager.Instance.GetFlag(
            flagToSetOnComplete
        );
    }

    private void PlayDialogue(
        DialogueData selectedDialogue,
        bool hasRequiredClue)
    {
        if (selectedDialogue == null ||
            selectedDialogue.lines == null ||
            selectedDialogue.lines.Length == 0)
        {
            OnDialogueComplete(hasRequiredClue);
            return;
        }

        if (DialogueRunner.Instance == null)
        {
            UnityEngine.Debug.LogWarning(
                "[NPCInteractable] '" +
                gameObject.name +
                "' tried to play dialogue but DialogueRunner.Instance is null."
            );

            return;
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }

        DialogueRunner.Instance.Play(
            selectedDialogue,
            () => OnDialogueComplete(hasRequiredClue)
        );
    }

    private void OnDialogueComplete(bool hasRequiredClue)
    {
        if (hasRequiredClue)
        {
            GiveReward();

            SetCompletionFlag();

            if (deactivateSelfOnComplete)
            {
                gameObject.SetActive(false);
            }
        }

        if (!string.IsNullOrEmpty(sceneToLoadOnComplete))
        {
            GameManager.Instance?.LoadScene(
                sceneToLoadOnComplete
            );
        }
        else
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.CanMove = true;
            }
        }
    }

    private void GiveReward()
    {
        if (itemToGrantOnComplete == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(
            itemToGrantOnComplete.objectID))
        {
            return;
        }

        BagUI.Instance?.RegisterClueDisplayName(
            itemToGrantOnComplete.objectID,
            itemToGrantOnComplete.displayName
        );

        GameManager.Instance?.AddClue(
            itemToGrantOnComplete.objectID
        );
    }

    private void SetCompletionFlag()
    {
        if (string.IsNullOrEmpty(flagToSetOnComplete))
        {
            return;
        }

        GameManager.Instance?.SetFlag(
            flagToSetOnComplete,
            true
        );
    }
}
