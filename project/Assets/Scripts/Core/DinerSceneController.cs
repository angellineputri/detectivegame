using System.Collections;
using UnityEngine;

public class DinerSceneController : MonoBehaviour
{
    [Header("Outcome Table")]
    [SerializeField] ClueOutcomeTable dinerOutcomeTable;

    [Header("NPCs")]
    [SerializeField] NPCInteractable exGF;
    [SerializeField] NPCInteractable headChef;

    [Header("Marcus Auto-Dialogue")]
    [SerializeField] DialogueData marcusDialogue;
    [SerializeField] float marcusPauseBeforeDialogue = 1f;

    [Header("Vivian Decision Panel")]
    [SerializeField] string vivianDecisionTitle = "Where to next?";

    [Header("ExGF Exit Walk")]
    [SerializeField] Transform exGFExitPoint;
    [SerializeField] float exGFPlayerAvoidanceRadius = 1.2f;

    [Header("Scene Obstacles")]
    [SerializeField] Transform[] sceneObstacles;
    [SerializeField] float obstacleRadius = 0.8f;

    [Header("Chef Walk-in")]
    [SerializeField] Transform chefWalkInStart;
    [SerializeField] Vector2 chefStandOffset = new Vector2(1.5f, 0f);

    [Header("Exit Walk")]
    [SerializeField] Transform doorExitPoint;
    [SerializeField] float exitWalkStaggerDelay = 0.5f;

    [Header("Movement")]
    [SerializeField] float characterWalkSpeed = 3f;

    [Header("Debug")]
    public bool useWalkAnimations = true;

    static readonly string[] DinerClueIDs =
    {
        "diner_corner_photo",
        "diner_reservation_book",
        "diner_jukebox",
        "diner_trashbin",
    };

    const string AllCluesFlag  = "allDinerCluesFound";
    const string ExGFFlag      = "hasSpokenToExGF";
    const string HeadChefFlag  = "hasSpokenToHeadChef";

    bool _allCluesUnlocked;
    bool _marcusSequenceStarted;
    bool _exitSequenceStarted;

    void Start()
    {
        BagUI.Instance?.SetOutcomeTable(dinerOutcomeTable);

        if (headChef != null)
        {
            headChef.gameObject.SetActive(false);
        }

        foreach (Interactable i in FindObjectsOfType<Interactable>())
        {
            i.RefreshActiveState();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClueAdded += OnClueAdded;
        }

        CheckAllCluesCollected();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClueAdded -= OnClueAdded;
        }
    }

    void OnClueAdded(string _)
    {
        if (!_allCluesUnlocked)
        {
            CheckAllCluesCollected();
        }
    }

    void CheckAllCluesCollected()
    {
        if (_allCluesUnlocked || GameManager.Instance == null) return;

        foreach (string id in DinerClueIDs)
        {
            if (!GameManager.Instance.HasClue(id)) return;
        }

        _allCluesUnlocked = true;
        GameManager.Instance.SetFlag(AllCluesFlag, true);
        exGF?.RefreshActiveState();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        if (!_marcusSequenceStarted && GameManager.Instance.GetFlag(ExGFFlag))
        {
            _marcusSequenceStarted = true;
            StartVivianDecision();
        }

        if (_marcusSequenceStarted && !_exitSequenceStarted && GameManager.Instance.GetFlag(HeadChefFlag))
        {
            _exitSequenceStarted = true;
            StartCoroutine(ExitSequence());
        }
    }

    void StartVivianDecision()
    {
        BagUI.Instance?.ForceOpenForDecision(vivianDecisionTitle, () => StartCoroutine(MarcusAndChefSequence()));
    }

    IEnumerator MarcusAndChefSequence()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }

        if (exGF != null && exGFExitPoint != null)
        {
            if (useWalkAnimations)
            {
                StartCoroutine(ExGFExitWalk());
            }
            else
            {
                exGF.gameObject.SetActive(false);
            }
        }

        yield return new WaitForSeconds(marcusPauseBeforeDialogue);

        if (marcusDialogue != null && DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(marcusDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        Vector3 chefDestination;
        if (PlayerController.Instance != null)
        {
            chefDestination = PlayerController.Instance.transform.position + (Vector3)chefStandOffset;
        }
        else if (chefWalkInStart != null)
        {
            chefDestination = chefWalkInStart.position;
        }
        else
        {
            chefDestination = Vector3.zero;
        }

        if (headChef != null)
        {
            headChef.gameObject.SetActive(true);
            if (chefWalkInStart != null)
            {
                headChef.transform.position = chefWalkInStart.position;
            }
        }

        Collider2D chefCollider = headChef != null ? headChef.GetComponent<Collider2D>() : null;
        if (chefCollider != null)
        {
            chefCollider.enabled = false;
        }

        if (headChef != null)
        {
            if (useWalkAnimations)
            {
                yield return StartCoroutine(CharacterMover.Walk(headChef.transform, chefDestination, characterWalkSpeed, sceneObstacles, obstacleRadius));
            }
            else
            {
                headChef.transform.position = chefDestination;
            }
        }

        if (chefCollider != null)
        {
            chefCollider.enabled = true;
        }

        headChef?.TriggerInteract();
    }

    IEnumerator ExitSequence()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }

        if (doorExitPoint == null)
        {
            Debug.LogError("[DinerSceneController] doorExitPoint is not assigned.");
        }
        else if (characterWalkSpeed <= 0f)
        {
            Debug.LogError("[DinerSceneController] characterWalkSpeed is 0 or negative.");
        }
        else if (useWalkAnimations)
        {
            if (headChef != null)
            {
                StartCoroutine(CharacterMover.WalkAndDeactivate(headChef.transform, doorExitPoint.position, characterWalkSpeed));
            }

            yield return new WaitForSeconds(exitWalkStaggerDelay);

            if (PlayerController.Instance != null)
            {
                yield return StartCoroutine(CharacterMover.Walk(PlayerController.Instance.transform, doorExitPoint.position, characterWalkSpeed));
            }
        }
        else
        {
            if (headChef != null)
            {
                headChef.transform.position = doorExitPoint.position;
            }
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.transform.position = doorExitPoint.position;
            }
        }

        if (headChef != null)
        {
            headChef.gameObject.SetActive(false);
        }
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.gameObject.SetActive(false);
        }

        GameManager.Instance?.LoadScene("Kitchen");
    }

    IEnumerator ExGFExitWalk()
    {
        CharacterMover.Obstacle[] sceneObs = CharacterMover.FromTransforms(sceneObstacles, obstacleRadius);
        CharacterMover.Obstacle[] allObs = new CharacterMover.Obstacle[sceneObs.Length + 1];
        sceneObs.CopyTo(allObs, 0);
        allObs[sceneObs.Length] = CharacterMover.Obstacle.FromTransform(PlayerController.Instance?.transform, exGFPlayerAvoidanceRadius);

        yield return StartCoroutine(CharacterMover.Walk(exGF.transform, exGFExitPoint.position, characterWalkSpeed, allObs));
    }
}
