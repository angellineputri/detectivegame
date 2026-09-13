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
        noticeText  = "A worn journal buried under some junk in the drawer. Someone tried hard to hide it.";
        revealText  = "One passage is underlined again and again. It is full of anger aimed at someone he only calls \"CI.\" He says he is ready to expose a huge secret, but he is waiting for the right moment.";
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
