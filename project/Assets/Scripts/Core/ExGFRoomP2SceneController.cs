using System.Collections;
using UnityEngine;

// Playthrough 2 — Vivian's bedroom.
// Collect-first, resolve-later pattern (same as Diner/Kitchen P1 transitions):
//   1. Player explores and picks up all 4 objects — each just adds its clue to the bag.
//   2. Once all 4 are in the bag, a "thinking" Marcus dialogue plays automatically.
//   3. After that dialogue, ForceOpenForDecision opens the bag panel.
//   4. Player picks one item — outcome table resolves it:
//        bed / nightstand / wardrobe → GameOver
//        sticky note → TriggerDialogue (reaction line) → LoadScene ExGF_Diner_P2
//
// Bag starts empty — AdvancePlaythrough() cleared it at the end of P1.
public class ExGFRoomP2SceneController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] ClueOutcomeTable bedroomP2OutcomeTable;

    [Header("Post-Collection Thinking Dialogue")]
    [Tooltip("Plays automatically once all four bedroom clues are collected.")]
    [SerializeField] DialogueData thinkingDialogue;
    [SerializeField] float thinkingDelay = 1f;

    [Header("Bag Decision Panel")]
    [SerializeField] string decisionTitle = "What stands out?";

    static readonly string[] ExRoomClueIDs =
    {
        "exgf_bed",
        "exgf_nightstand",
        "exgf_wardrobe",
        "exgf_vanity",
    };

    bool _allCluesFound;
    bool _decisionOpened;

    void Start()
    {
        BagUI.Instance?.SetOutcomeTable(bedroomP2OutcomeTable);

        // Activate any P2-gated interactables (sticky note has requiredPlaythrough = 2).
        foreach (Interactable i in FindObjectsOfType<Interactable>())
        {
            i.RefreshActiveState();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClueAdded += OnClueAdded;
        }

        // If the player re-enters this scene after already collecting everything, open the panel.
        if (AllCluesCollected())
        {
            _allCluesFound   = true;
            _decisionOpened  = true;
            BagUI.Instance?.ForceOpenForDecision(decisionTitle, null);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClueAdded -= OnClueAdded;
        }
    }

    void OnClueAdded(string clueID)
    {
        if (_allCluesFound) return;
        if (!AllCluesCollected()) return;

        _allCluesFound = true;
        StartCoroutine(ThinkingSequence());
    }

    bool AllCluesCollected()
    {
        if (GameManager.Instance == null) return false;
        foreach (string id in ExRoomClueIDs)
        {
            if (!GameManager.Instance.HasClue(id)) return false;
        }
        return true;
    }

    IEnumerator ThinkingSequence()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }

        yield return new WaitForSeconds(thinkingDelay);

        if (thinkingDialogue != null && DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(thinkingDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        _decisionOpened = true;
        BagUI.Instance?.ForceOpenForDecision(decisionTitle, null);
    }
}
