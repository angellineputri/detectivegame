using UnityEngine;

[System.Serializable]
public class ItemGrant
{
    public string objectID;
    public string displayName;
    [TextArea(2, 4)] public string clueDescription;
}

public class NPCInteractable : Interactable
{
    [Header("NPC Dialogue")]
    public DialogueData dialogue;

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

    [Header("On Dialogue Complete")]
    [Tooltip("Flag to set in GameManager when dialogue finishes. Leave empty to skip.")]
    public string flagToSetOnComplete;

    [Tooltip("Optional item to add to the Bag when dialogue finishes.")]
    public ItemGrant itemToGrantOnComplete;

    [Tooltip("Scene to load after dialogue finishes. Leave empty to stay in current scene.")]
    public string sceneToLoadOnComplete;

    [Tooltip("If true, this NPC's GameObject deactivates itself after dialogue completes.")]
    public bool deactivateSelfOnComplete;

    protected override void OnInteract()
    {
        if (!string.IsNullOrEmpty(flagToSetOnComplete) && GameManager.Instance != null && GameManager.Instance.GetFlag(flagToSetOnComplete))
        {
            return;
        }

        if (!string.IsNullOrEmpty(objectID))
        {
            GameManager.Instance?.AddClue(objectID);
        }

        if (dialogue != null && dialogue.lines.Length > 0)
        {
            if (DialogueRunner.Instance == null)
            {
                Debug.LogWarning("[NPCInteractable] '" + gameObject.name + "' tried to play dialogue but DialogueRunner.Instance is null.");
                return;
            }

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.CanMove = false;
            }

            DialogueRunner.Instance.Play(dialogue, OnDialogueComplete);
        }
        else
        {
            OnDialogueComplete();
        }
    }

    void OnDialogueComplete()
    {
        if (!string.IsNullOrEmpty(flagToSetOnComplete))
        {
            GameManager.Instance?.SetFlag(flagToSetOnComplete, true);
        }

        if (!string.IsNullOrEmpty(itemToGrantOnComplete?.objectID))
        {
            BagUI.Instance?.RegisterClueDisplayName(itemToGrantOnComplete.objectID, itemToGrantOnComplete.displayName);
            GameManager.Instance?.AddClue(itemToGrantOnComplete.objectID);
        }

        if (!string.IsNullOrEmpty(sceneToLoadOnComplete))
        {
            GameManager.Instance?.LoadScene(sceneToLoadOnComplete);
        }
        else if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = true;
        }

        if (deactivateSelfOnComplete)
        {
            gameObject.SetActive(false);
        }
    }
}
