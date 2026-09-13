using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class ApartmentSceneController : MonoBehaviour
{
    [Header("Outcome Table")]
    [Tooltip("Outcome table used by the Bag UI for the Assistant apartment.")]
    [SerializeField] ClueOutcomeTable apartmentOutcomeTable;

    [Header("Assistant")]
    [Tooltip("Assistant NPC in the apartment.")]
    [SerializeField] NPCInteractable assistant;

    [Header("Decision Panel")]
    [Tooltip("Title shown when the player is asked to decide what evidence to use.")]
    [SerializeField] string decisionTitle = "What do you want to investigate?";

    [Header("Opening")]
    [Tooltip("Optional dialogue played when entering the apartment.")]
    [SerializeField] DialogueData openingDialogue;

    [Tooltip("Delay before the opening dialogue starts.")]
    [SerializeField] float openingDialogueDelay = 0.5f;

    [Header("Scene Navigation")]
    [Tooltip("Scene loaded when the apartment sequence is completed.")]
    [SerializeField] string nextSceneName = "office";

    [Header("Phone Clue Dialogue Chain")]
    [Tooltip("Chief commentary played right after the phone clue is collected.")]
    [SerializeField] DialogueData phoneChiefCommentaryDialogue;

    [Tooltip("Player/Assistant dialogue triggered when the player manually interacts after the chief commentary.")]
    [SerializeField] DialogueData phoneAssistantDialogue;

    [Tooltip("Chief and Player closing exchange after the assistant dialogue.")]
    [SerializeField] DialogueData phoneChiefClosingDialogue;

bool _decisionStarted;
    bool _apartmentCompleted;
    bool _chainStarted;

    void Awake()
    {
        BagUI.Instance?.ClearBag();
    }

    void Start()
    {

        CaseBoardManager.Instance?.Assistant_P1_Pin();

        if (BagUI.Instance != null)
        {
            BagUI.Instance.SetOutcomeTable(apartmentOutcomeTable);
            BagUI.Instance.SetHudVisible(true);
        }

        RefreshInteractables();

        RestorePendingSpawn();

        if (openingDialogue != null)
        {
            StartCoroutine(OpeningSequence());
        }
        else
        {
            EnablePlayerMovement();
        }

    }

    void OnEnable()
    {
        ItemSelectionUI.SceneLoadInterceptHandler =
            HandleSceneLoadIntercept;
        ItemSelectionUI.PreDialogueWalkHandler =
            HandlePhoneDialogueWalk;
    }

    void OnDisable()
    {
        if (ItemSelectionUI.SceneLoadInterceptHandler ==
            HandleSceneLoadIntercept)
        {
            ItemSelectionUI.SceneLoadInterceptHandler = null;
        }

        if (ItemSelectionUI.PreDialogueWalkHandler ==
            HandlePhoneDialogueWalk)
        {
            ItemSelectionUI.PreDialogueWalkHandler = null;
        }
    }

    IEnumerator OpeningSequence()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }

        yield return new WaitForSeconds(openingDialogueDelay);

        if (openingDialogue != null &&
            DialogueRunner.Instance != null)
        {
            bool finished = false;

            DialogueRunner.Instance.Play(
                openingDialogue,
                () => finished = true
            );

            yield return new WaitUntil(() => finished);
        }

        EnablePlayerMovement();
    }

    void EnablePlayerMovement()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = true;
        }
    }

    public void SetOutcomeTable()
    {
        if (BagUI.Instance == null)
        {
            return;
        }

        BagUI.Instance.SetOutcomeTable(apartmentOutcomeTable);

    }

    public void StartDecision()
    {
        if (_decisionStarted)
        {
            return;
        }

        _decisionStarted = true;

        if (BagUI.Instance != null)
        {
            BagUI.Instance.ForceOpenForDecision(
                decisionTitle,
                OnDecisionComplete
            );

        }
        else
        {
            _decisionStarted = false;
        }
    }

    void OnDecisionComplete()
    {

        _decisionStarted = false;
        CheckApartmentCompletion();
    }

    void RefreshInteractables()
    {
        Interactable[] interactables =
            FindObjectsOfType<Interactable>();

        foreach (Interactable interactable in interactables)
        {
            if (interactable != null)
            {
                interactable.RefreshActiveState();
            }
        }
    }

    void CheckApartmentCompletion()
    {
        if (_apartmentCompleted)
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.GetFlag("assistant_keys_given"))
        {
            _apartmentCompleted = true;

        }
    }

    void HandleSceneLoadIntercept(
        string clueID,
        System.Action proceedWithLoad)
    {

        if (PlayerController.Instance != null &&
            GameManager.Instance != null)
        {
            GameManager.Instance.SetPendingSpawn(
                PlayerController.Instance.transform.position
            );
        }

        proceedWithLoad?.Invoke();
    }

    public bool CanLeave()
    {
        if (GameManager.Instance == null)
        {
            return false;
        }

        return GameManager.Instance.GetFlag(
            "assistant_keys_given"
        );
    }

    public void GoToNextScene()
    {
        if (!CanLeave())
        {
            return;
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {

            return;
        }

        SavePlayerPosition();

GameManager.Instance.LoadScene(nextSceneName);
    }

    void SavePlayerPosition()
    {
        if (GameManager.Instance == null ||
            PlayerController.Instance == null)
        {
            return;
        }

        GameManager.Instance.SetPendingSpawn(
            PlayerController.Instance.transform.position
        );
    }

    void RestorePendingSpawn()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (!GameManager.Instance.HasPendingSpawn)
        {
            return;
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.transform.position =
                GameManager.Instance.PendingSpawnPosition;

        }

        GameManager.Instance.ConsumePendingSpawn();
    }

    void HandlePhoneDialogueWalk(string clueID, System.Action onContinue)
    {
        if (clueID != "phone" || _chainStarted)
        {
            onContinue?.Invoke();
            return;
        }

        _chainStarted = true;
        StartCoroutine(PhoneDialogueChain());

    }

    IEnumerator PhoneDialogueChain()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.CanMove = false;

        if (phoneChiefCommentaryDialogue != null &&
            DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(
                phoneChiefCommentaryDialogue,
                () => done = true
            );
            yield return new WaitUntil(() => done);
        }

        yield return new WaitForSeconds(0.5f);

        bool assistantDialogueDone = false;
        if (assistant != null)
        {
            assistant.overrideDialogue = phoneAssistantDialogue;
            assistant.onOverrideDialogueComplete = () => assistantDialogueDone = true;
        }

        if (PlayerController.Instance != null)
            PlayerController.Instance.CanMove = true;

        yield return new WaitUntil(() => assistantDialogueDone);

        yield return new WaitForSeconds(0.5f);

        if (GameManager.Instance != null)
        {
            BagUI.Instance?.RegisterClueDisplayName(
                "assistant_keys",
                "Assistant's Keys"
            );
            GameManager.Instance.AddClue("assistant_keys");
            GameManager.Instance.SetFlag("assistant_keys_given", true);
        }

        if (phoneChiefClosingDialogue != null &&
            DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(
                phoneChiefClosingDialogue,
                () => done = true
            );
            yield return new WaitUntil(() => done);
        }

        GameManager.Instance?.LoadScene(nextSceneName);
    }

    public bool HasApartmentEntranceClue()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.HasClue(
                   "apartment_entrance"
               );
    }

    public bool HasPhoneClue()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.HasClue(
                   "phone"
               );
    }

    public bool HasAssistantKeys()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.HasClue(
                   "assistant_keys"
               );
    }

    public bool HasReceivedAssistantKeys()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.GetFlag(
                   "assistant_keys_given"
               );
    }

}
