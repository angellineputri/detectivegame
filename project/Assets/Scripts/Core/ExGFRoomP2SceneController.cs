using System.Collections;
using UnityEngine;

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

    void Start()
    {
        CaseBoardManager.Instance?.ExGf_P2_Pin();

        BagUI.Instance?.SetOutcomeTable(bedroomP2OutcomeTable);

        foreach (Interactable i in FindObjectsOfType<Interactable>())
        {
            i.RefreshActiveState();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClueAdded += OnClueAdded;
        }

        if (AllCluesCollected())
        {
            _allCluesFound   = true;
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

        BagUI.Instance?.ForceOpenForDecision(decisionTitle, null);
    }
}
