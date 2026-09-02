using System.Collections;
using System.Collections.Generic;
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

    [Header("Door")]
    public Door door;

    [Header("Debug")]
    public bool useWalkAnimations = true;

    static readonly string[] DinerClueIDs =
    {
        "diner_corner_photo",
        "diner_reservation_book",
        "diner_jukebox",
        "diner_trashbin",
    };

    const string AllCluesFlag = "allDinerCluesFound";
    const string ExGFFlag     = "hasSpokenToExGF";
    const string HeadChefFlag = "hasSpokenToHeadChef";

    bool _allCluesUnlocked;
    bool _marcusSequenceStarted;
    bool _exitSequenceStarted;

    void Start()
    {
        if (BagUI.Instance != null)
        {
            BagUI.Instance.SetOutcomeTable(dinerOutcomeTable);
        }

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

        if (exGF != null)
        {
            exGF.RefreshActiveState();
        }
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
            if (door != null)
            {
                door.Open();
            }
            StartCoroutine(ExitSequence());
        }
    }

    void StartVivianDecision()
    {
        if (BagUI.Instance != null)
        {
            BagUI.Instance.ForceOpenForDecision(vivianDecisionTitle, () => StartCoroutine(MarcusAndChefSequence()));
        }
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
            Vector3 playerPos = PlayerController.Instance.transform.position;
            Vector3 chefStart = chefWalkInStart != null ? chefWalkInStart.position : playerPos;
            Vector3 approachDir = (playerPos - chefStart).normalized;
            chefDestination = playerPos - approachDir;
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
                if (GridPathfinder.Instance != null)
                {
                    Vector2? playerPos2D = null;
                    if (PlayerController.Instance != null)
                    {
                        playerPos2D = PlayerController.Instance.transform.position;
                    }
                    List<Vector2> chefPath = GridPathfinder.Instance.FindPath(headChef.transform.position, chefDestination, playerPos2D);
                    yield return StartCoroutine(CharacterMover.WalkPath(headChef.transform, chefPath, characterWalkSpeed));
                }
                else
                {
                    yield return StartCoroutine(CharacterMover.Walk(headChef.transform, chefDestination, characterWalkSpeed, sceneObstacles, obstacleRadius));
                }
            }
            else
            {
                headChef.transform.position = chefDestination;
            }
        }

        if (PlayerController.Instance != null && headChef != null)
        {
            Vector2 toChef = (Vector2)(headChef.transform.position - PlayerController.Instance.transform.position);
            PlayerController.Instance.FaceDirection(toChef);
        }

        yield return new WaitForSeconds(0.5f);

        if (chefCollider != null)
        {
            chefCollider.enabled = true;
        }

        if (headChef != null)
        {
            headChef.TriggerInteract();
        }
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
            Collider2D chefExitCollider = headChef != null ? headChef.GetComponent<Collider2D>() : null;
            Collider2D playerExitCollider = PlayerController.Instance != null ? PlayerController.Instance.GetComponent<Collider2D>() : null;
            if (chefExitCollider != null && playerExitCollider != null)
            {
                Physics2D.IgnoreCollision(chefExitCollider, playerExitCollider, true);
            }

            bool chefExited = (headChef == null);
            bool playerExited = (PlayerController.Instance == null);

            if (headChef != null)
            {
                StartCoroutine(ExitWalkAndDeactivate(headChef.transform, headChef.gameObject, () => chefExited = true));
            }

            yield return new WaitForSeconds(exitWalkStaggerDelay);

            if (PlayerController.Instance != null)
            {
                StartCoroutine(ExitWalkAndDeactivate(PlayerController.Instance.transform, PlayerController.Instance.gameObject, () => playerExited = true));
            }

            yield return new WaitUntil(() => chefExited && playerExited);
        }
        else
        {
            if (headChef != null)
            {
                headChef.transform.position = doorExitPoint.position;
                headChef.gameObject.SetActive(false);
            }
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.transform.position = doorExitPoint.position;
                PlayerController.Instance.gameObject.SetActive(false);
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene("ExGF_Kitchen_P1");
        }
    }

    IEnumerator ExitWalkAndDeactivate(Transform obj, GameObject go, System.Action onDone)
    {
        if (GridPathfinder.Instance != null)
        {
            List<Vector2> path = GridPathfinder.Instance.FindPath(obj.position, doorExitPoint.position);
            yield return StartCoroutine(CharacterMover.WalkPath(obj, path, characterWalkSpeed));
        }
        else
        {
            yield return StartCoroutine(CharacterMover.Walk(obj, doorExitPoint.position, characterWalkSpeed));
        }
        go.SetActive(false);
        if (onDone != null)
        {
            onDone();
        }
    }

    IEnumerator ExGFExitWalk()
    {
        Collider2D exGFCollider = exGF.GetComponent<Collider2D>();
        Collider2D playerCollider = PlayerController.Instance != null ? PlayerController.Instance.GetComponent<Collider2D>() : null;
        if (exGFCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(exGFCollider, playerCollider, true);
        }

        if (GridPathfinder.Instance != null)
        {
            List<Vector2> exGFPath = GridPathfinder.Instance.FindPath(exGF.transform.position, exGFExitPoint.position);
            yield return StartCoroutine(CharacterMover.WalkPath(exGF.transform, exGFPath, characterWalkSpeed));
        }
        else
        {
            CharacterMover.Obstacle[] sceneObs = CharacterMover.FromTransforms(sceneObstacles, obstacleRadius);
            CharacterMover.Obstacle[] allObs = new CharacterMover.Obstacle[sceneObs.Length + 1];
            sceneObs.CopyTo(allObs, 0);
            Transform playerTransform = PlayerController.Instance != null ? PlayerController.Instance.transform : null;
            allObs[sceneObs.Length] = CharacterMover.Obstacle.FromTransform(playerTransform, exGFPlayerAvoidanceRadius);
            yield return StartCoroutine(CharacterMover.Walk(exGF.transform, exGFExitPoint.position, characterWalkSpeed, allObs));
        }

        yield return new WaitForSeconds(0.5f);

        if (exGFCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(exGFCollider, playerCollider, false);
        }
    }
}
