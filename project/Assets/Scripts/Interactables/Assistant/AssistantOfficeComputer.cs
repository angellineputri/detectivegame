using UnityEngine;

public class AssistantOfficeComputer : Interactable
{
    bool _used;

    void Reset()
    {
        objectID    = "assistant_computer";
        displayName = "Assistant's computer";
    }

    protected override void OnInteract()
    {
        if (_used) return;

        if (!string.IsNullOrEmpty(noticeText))
        {
            UIManager.Instance?.ShowCluePopup(
                noticeText,
                revealText,
                OnComputerRevealComplete
            );
        }
        else
        {
            OnComputerRevealComplete();
        }
    }

    void OnComputerRevealComplete()
    {
        _used = true;
        interactionLocked = true;

        if (!string.IsNullOrEmpty(objectID))
            GameManager.Instance?.AddClue(objectID);

        if (PlayerController.Instance != null)
            PlayerController.Instance.CanMove = true;
    }
}
