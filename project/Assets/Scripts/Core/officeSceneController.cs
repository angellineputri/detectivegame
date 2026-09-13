using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class OfficeSceneController : MonoBehaviour
{
    [Header("Outcome Table")]
    [Tooltip("Outcome table used by the Bag UI for the office.")]
    [SerializeField] ClueOutcomeTable apartmentOutcomeTable;

    [Header("NPCs")]
    [Tooltip("Assistant NPC in the office.")]
    [SerializeField] NPCInteractable npc_assistant;
    [SerializeField] NPCInteractable npc_IT_head;

    [Header("Office Computer")]
    [Tooltip("The assistant's computer interactable. Locked until IT head is talked to.")]
    [SerializeField] Interactable officeComputer;

    [Header("Decision Panel")]
    [Tooltip("Title shown when the player is asked to decide what evidence to use.")]
    [SerializeField] string decisionTitle = "What do you want to investigate?";

    [Header("Opening")]
    [Tooltip("Optional dialogue played when entering the apartment.")]
    [SerializeField] DialogueData openingDialogue;

    [Tooltip("Delay before the opening dialogue starts.")]
    [SerializeField] float openingDialogueDelay = 0.5f;

    [Header("Scene Navigation")]
    [Tooltip("Scene loaded after the assistant's dialogue ends.")]
    [SerializeField] string courtSceneName = "court";

    [Header("Debug")]
    [SerializeField] bool debugLogs = true;

    bool _decisionStarted;
    bool _apartmentCompleted;

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
        StartCoroutine(LateApplyOfficeGating());

        if (npc_assistant != null)
        {
            npc_assistant.sceneToLoadOnComplete = courtSceneName;
        }

        if (npc_IT_head != null)
        {
            npc_IT_head.onCompletionCallback = OnITHeadTalkedTo;
        }

        RestorePendingSpawn();

        if (openingDialogue != null)
        {
            StartCoroutine(OpeningSequence());
        }
        else
        {
            EnablePlayerMovement();
        }

        Log("Assistant apartment scene started.");
    }

    void OnEnable()
    {
        ItemSelectionUI.SceneLoadInterceptHandler =
            HandleSceneLoadIntercept;
        ItemSelectionUI.PostDialogueHandler =
            HandlePostDialogue;
    }

    void OnDisable()
    {
        if (ItemSelectionUI.SceneLoadInterceptHandler ==
            HandleSceneLoadIntercept)
        {
            ItemSelectionUI.SceneLoadInterceptHandler = null;
        }

        if (ItemSelectionUI.PostDialogueHandler ==
            HandlePostDialogue)
        {
            ItemSelectionUI.PostDialogueHandler = null;
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
            Log("BagUI is not available.");
            return;
        }

        BagUI.Instance.SetOutcomeTable(apartmentOutcomeTable);

        Log("Assistant apartment outcome table assigned.");
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

            Log("Apartment evidence decision opened.");
        }
        else
        {
            Log("BagUI is missing. Cannot open decision panel.");
            _decisionStarted = false;
        }
    }

    void OnDecisionComplete()
    {
        Log("Apartment evidence decision completed.");

        _decisionStarted = false;
        CheckApartmentCompletion();
    }

    public void OnComputerUsed()
    {
        Log("Computer used — unlocking assistant.");
        ApplyOfficeGating();
    }

    public void OnITHeadTalkedTo()
    {
        Log("IT head dialogue done — unlocking computer.");
        ApplyOfficeGating();
    }

    void HandlePostDialogue(string clueID)
    {
        if (clueID == "assistant_computer")
        {
            Log("assistant_computer bag move done — unlocking assistant.");
            ApplyOfficeGating();
        }
    }

    IEnumerator LateApplyOfficeGating()
    {
        yield return null;
        ApplyOfficeGating();
    }

    void ApplyOfficeGating()
    {
        bool hasITPermission =
            GameManager.Instance?.HasClue("IT_permission") ?? false;
        bool hasComputerClue =
            GameManager.Instance?.HasClue("assistant_computer") ?? false;

        SetLocked(officeComputer, !hasITPermission);
        SetLocked(npc_assistant, !hasComputerClue);

        Log($"Office gating — computer locked: {!hasITPermission}, assistant locked: {!hasComputerClue}");
    }

    void SetLocked(Interactable target, bool locked)
    {
        if (target == null) return;
        target.interactionLocked = locked;
    }

    public void RefreshInteractables()
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

            Log("Assistant apartment sequence completed.");
        }
    }

    void HandleSceneLoadIntercept(
        string clueID,
        System.Action proceedWithLoad)
    {
        Log("Scene load requested by outcome: " + clueID);

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
            Log("Cannot leave apartment yet.");
            return;
        }

        if (string.IsNullOrEmpty(courtSceneName))
        {
            UnityEngine.Debug.LogWarning(
                "[ApartmentSceneController] " +
                "No next scene has been assigned."
            );

            return;
        }

        SavePlayerPosition();

        Log("Loading next Assistant scene: " + courtSceneName);

        GameManager.Instance.LoadScene(courtSceneName);
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

            Log("Restored player pending spawn position.");
        }

        GameManager.Instance.ConsumePendingSpawn();
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

    void Log(string message)
    {
        if (debugLogs)
        {
            UnityEngine.Debug.Log(
                "[ApartmentSceneController] " +
                message
            );
        }
    }
}
