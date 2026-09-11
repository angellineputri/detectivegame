using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class officeSceneController : MonoBehaviour
{
    [Header("Outcome Table")]
    [Tooltip("Outcome table used by the Bag UI for the office.")]
    [SerializeField] ClueOutcomeTable apartmentOutcomeTable;

    [Header("Assistant")]
    [Tooltip("Assistant NPC in the apartment.")]
    [SerializeField] NPCInteractable npc_assistant;
    [SerializeField] NPCInteractable npc_IT_head;

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

    [Header("Debug")]
    [SerializeField] bool debugLogs = true;

    bool _decisionStarted;
    bool _apartmentCompleted;


    // ---------------------------------------------------------
    // INITIALISATION
    // ---------------------------------------------------------

    void Awake()
    {
        BagUI.Instance?.ClearBag();
    }


    void Start()
    {
        // -----------------------------------------------------
        // Mark Assistant P1 as the active case.
        // -----------------------------------------------------

        CaseBoardManager.Instance?.Assistant_P1_Pin();


        // -----------------------------------------------------
        // Give the Bag UI this scene's outcome table.
        // This is the same system used by DinerSceneController.
        // -----------------------------------------------------

        if (BagUI.Instance != null)
        {
            BagUI.Instance.SetOutcomeTable(apartmentOutcomeTable);
            BagUI.Instance.SetHudVisible(true);
        }


        // -----------------------------------------------------
        // Refresh interactables so clue requirements are checked.
        // -----------------------------------------------------

        RefreshInteractables();


        // -----------------------------------------------------
        // Restore the player's position if this scene was entered
        // using a pending spawn position.
        // -----------------------------------------------------

        RestorePendingSpawn();


        // -----------------------------------------------------
        // Optional opening dialogue.
        // -----------------------------------------------------

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
        // -----------------------------------------------------
        // Allow ItemSelectionUI to intercept scene loads if
        // the outcome table requires special handling.
        // -----------------------------------------------------

        ItemSelectionUI.SceneLoadInterceptHandler =
            HandleSceneLoadIntercept;
    }


    void OnDisable()
    {
        if (ItemSelectionUI.SceneLoadInterceptHandler ==
            HandleSceneLoadIntercept)
        {
            ItemSelectionUI.SceneLoadInterceptHandler = null;
        }
    }


    // ---------------------------------------------------------
    // OPENING SEQUENCE
    // ---------------------------------------------------------

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


    // ---------------------------------------------------------
    // OUTCOME TABLE
    // ---------------------------------------------------------

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


    // ---------------------------------------------------------
    // DECISION SYSTEM
    // ---------------------------------------------------------

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


    // ---------------------------------------------------------
    // CLUE HANDLING
    // ---------------------------------------------------------

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

        // The Assistant gives the keys after the required clue
        // has been found and their dialogue has completed.
        if (GameManager.Instance.GetFlag("assistant_keys_given"))
        {
            _apartmentCompleted = true;

            Log("Assistant apartment sequence completed.");
        }
    }


    // ---------------------------------------------------------
    // SCENE LOAD INTERCEPT
    // ---------------------------------------------------------

    void HandleSceneLoadIntercept(
        string clueID,
        System.Action proceedWithLoad)
    {
        Log("Scene load requested by outcome: " + clueID);

        // Save the player's position before leaving the scene.
        if (PlayerController.Instance != null &&
            GameManager.Instance != null)
        {
            GameManager.Instance.SetPendingSpawn(
                PlayerController.Instance.transform.position
            );
        }

        proceedWithLoad?.Invoke();
    }


    // ---------------------------------------------------------
    // NAVIGATION
    // ---------------------------------------------------------

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

        if (string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.Debug.LogWarning(
                "[ApartmentSceneController] " +
                "No next scene has been assigned."
            );

            return;
        }

        SavePlayerPosition();

        Log("Loading next Assistant scene: " + nextSceneName);

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


    // ---------------------------------------------------------
    // SPAWN
    // ---------------------------------------------------------

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


    // ---------------------------------------------------------
    // HELPER FUNCTIONS
    // ---------------------------------------------------------

    public bool HasApartmentEntranceClue()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.HasClue(
                   "apartment_entrance"
               );
    }


    public bool HasBedroomDrawerClue()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.HasClue(
                   "bedroom_drawer"
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
