using System.Collections;
using UnityEngine;

public class VictimApartmentFinalSceneController : MonoBehaviour
{
    [Header("Required Clues (all must be collected)")]
    [SerializeField] string[] requiredClues =
    {
        "final_photo",
        "final_note",
        "victim_journal_ci",
        "victim_sofa_blood"
    };

    [Header("Outcome Table")]
    [Tooltip("Every entry should be GameOver — any premature 'make a move' ends the game.")]
    [SerializeField] ClueOutcomeTable outcomeTable;

    [Header("Completion")]
    [Tooltip("Plays once all four clues are collected.")]
    [SerializeField] DialogueData headChefP3Call;
    [Tooltip("Scene to load after the phone call.")]
    [SerializeField] string nextSceneName = "ExGF_ExRoom_P3";
    [SerializeField] float pauseBeforeCall = 0.3f;

    [Header("Opening Description (optional)")]
    [SerializeField, TextArea(2, 4)] string openingDescription;
    [SerializeField, TextArea(2, 4)] string openingRevealText;

    bool _advancing;

    void Awake()
    {
        BagUI.Instance?.ClearBag();
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnClueAdded += HandleClueAdded;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnClueAdded -= HandleClueAdded;
    }

    void Start()
    {
        BagUI.Instance?.SetOutcomeTable(outcomeTable);
        BagUI.Instance?.SetHudVisible(true);

        if (PlayerController.Instance != null)
            PlayerController.Instance.CanMove = false;

        StartCoroutine(OpeningSequence());
    }

    IEnumerator OpeningSequence()
    {
        yield return new WaitForSeconds(0.5f);

        if (!string.IsNullOrEmpty(openingDescription) && UIManager.Instance != null)
        {
            bool done = false;
            UIManager.Instance.ShowCluePopup(openingDescription, openingRevealText, () => done = true);
            yield return new WaitUntil(() => done);
        }

        if (PlayerController.Instance != null)
            PlayerController.Instance.CanMove = true;

        CheckAllCollected();
    }

    void HandleClueAdded(string clueID) => CheckAllCollected();

    void CheckAllCollected()
    {
        if (_advancing || GameManager.Instance == null) return;

        foreach (string clue in requiredClues)
        {
            if (!GameManager.Instance.HasClue(clue)) return;
        }

        _advancing = true;
        StartCoroutine(AdvanceSequence());
    }

    IEnumerator AdvanceSequence()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.CanMove = false;

        yield return null;
        yield return new WaitUntil(() =>
            (DialogueRunner.Instance == null || !DialogueRunner.Instance.IsPlaying) &&
            (UIManager.Instance == null || !UIManager.Instance.IsPopupVisible));

        yield return new WaitForSeconds(pauseBeforeCall);

        if (PlayerController.Instance != null)
            PlayerController.Instance.CanMove = false;

        if (headChefP3Call != null && DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(headChefP3Call, () => done = true);
            yield return new WaitUntil(() => done);
        }

        if (!string.IsNullOrEmpty(nextSceneName))
            GameManager.Instance?.LoadScene(nextSceneName);
    }
}
