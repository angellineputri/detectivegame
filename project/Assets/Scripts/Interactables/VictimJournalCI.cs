using System.Collections;
using UnityEngine;

public class VictimJournalCI : Interactable
{
    [Header("Thinking Dialogue")]
    [Tooltip("Plays after the journal clue is discovered.")]
    [SerializeField] DialogueData thinkingDialogue;
    [Tooltip("Delay after the clue reveal closes, before the thinking dialogue starts.")]
    [SerializeField] float pauseBeforeThinking = 0.5f;

    void Reset()
    {
        objectID    = "victim_journal_ci";
        displayName = "Victim's Journal";
        noticeText  = "A worn journal hidden well in the drawer, underneath some junks, as if its purposely hidden away..";
        revealText  = "One passage is underlined over and over — venom aimed at someone he only calls \"CI.\" He writes that he's ready to expose a huge secret, but is waiting for the right moment.";
    }

    protected override void OnInteract()
    {
        if (!string.IsNullOrEmpty(noticeText))
        {
            UIManager.Instance?.ShowCluePopup(noticeText, revealText, OnJournalRevealComplete);
        }
    }

    void OnJournalRevealComplete()
    {
        if (thinkingDialogue != null && DialogueRunner.Instance != null)
        {

            if (PlayerController.Instance != null) PlayerController.Instance.CanMove = false;
            StartCoroutine(PlayThinkingAfterDelay());
        }
        else
        {
            AddJournalClue();
        }
    }

    IEnumerator PlayThinkingAfterDelay()
    {
        yield return new WaitForSeconds(pauseBeforeThinking);
        DialogueRunner.Instance.Play(thinkingDialogue, OnThinkingComplete);
    }

    void OnThinkingComplete()
    {
        AddJournalClue();
        if (PlayerController.Instance != null) PlayerController.Instance.CanMove = true;
    }

    void AddJournalClue()
    {
        if (GameManager.Instance != null && !string.IsNullOrEmpty(objectID))
        {
            GameManager.Instance.AddClue(objectID);
        }
    }
}
