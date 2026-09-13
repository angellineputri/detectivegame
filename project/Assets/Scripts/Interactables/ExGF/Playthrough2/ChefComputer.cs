using System.Collections;
using UnityEngine;

public class ChefComputer : Interactable
{
    [Header("Playthrough 2 Text")]
    [Tooltip("Notice text shown on first interaction during Playthrough 2.")]
    [TextArea(2, 4)]
    public string playthrough2NoticeText;

    [Tooltip("Reveal text shown on second interaction during Playthrough 2.")]
    [TextArea(2, 4)]
    public string playthrough2RevealText;

    [Tooltip("Internal monologue played automatically after the P2 reveal popup closes.")]
    public DialogueData p2ThinkingDialogue;

    void Reset()
    {
        objectID    = "chef_computer";
        displayName = "Email";
        noticeText  = "The computer is still on, screen glowing in the dim room.";
        revealText  = "An email, unopened. Sender: unknown. \"I know you don't like him. Need help?\" Timestamp: the day of the murder.";

        playthrough2NoticeText = "The computer is still on. There is a new email visible.";
        playthrough2RevealText = "A new message. The sender address doesn't match anyone in this case. Someone else was involved.";
    }

    protected override void OnInteract()
    {
        if (GameManager.Instance?.CurrentPlaythrough == 2 && !string.IsNullOrEmpty(playthrough2NoticeText))
        {
            UIManager.Instance?.ShowCluePopup(playthrough2NoticeText, playthrough2RevealText, OnP2RevealComplete);
        }
        else
        {
            base.OnInteract();
        }
    }

    void OnP2RevealComplete()
    {
        if (!string.IsNullOrEmpty(objectID))
        {
            GameManager.Instance?.AddClue(objectID);
        }

        StartCoroutine(PlayP2ThinkingSequence());
    }

    IEnumerator PlayP2ThinkingSequence()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }

        if (p2ThinkingDialogue != null && DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(p2ThinkingDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }
        else
        {
        }

        GameManager.Instance?.LoadScene("ExGF_ExRoom_P2");
    }
}
