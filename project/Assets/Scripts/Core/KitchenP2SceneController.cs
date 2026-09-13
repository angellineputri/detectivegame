using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenP2SceneController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] ClueOutcomeTable kitchenP2OutcomeTable;

    [Header("Thinking Dialogue")]
    [SerializeField] DialogueData thinkingDialogue;
    [SerializeField] float thinkingDelay = 0.5f;

    [Header("Door")]
    [SerializeField] SilentDoor kitchenDoor;

    [Header("Movement")]
    [SerializeField] float characterWalkSpeed = 3f;

    const string KnifeClueID    = "bloody_knife_bagged";
    const string LabResultsID   = "lab_results";
    const string LabResultsName = "Lab Results";

    bool _exitStarted;

    void Start()
    {

        BagUI.Instance?.SetOutcomeTable(kitchenP2OutcomeTable);

        foreach (Interactable i in FindObjectsOfType<Interactable>())
        {
            i.RefreshActiveState();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClueAdded += OnClueAdded;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClueAdded -= OnClueAdded;
        }
    }

    void OnEnable()
    {
        ItemSelectionUI.SceneLoadInterceptHandler = HandleSceneLoadIntercept;
    }

    void OnDisable()
    {
        if (ItemSelectionUI.SceneLoadInterceptHandler == HandleSceneLoadIntercept)
        {
            ItemSelectionUI.SceneLoadInterceptHandler = null;
        }
    }

    void OnClueAdded(string clueID)
    {
        if (clueID == KnifeClueID && !_exitStarted)
        {
            _exitStarted = true;
            StartCoroutine(ThinkingAndExit());
        }
    }

    IEnumerator ThinkingAndExit()
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

        BagUI.Instance?.RegisterClueDisplayName(LabResultsID, LabResultsName);
        GameManager.Instance?.AddClue(LabResultsID);

        yield return StartCoroutine(WalkToDoorAndExit());
    }

    void HandleSceneLoadIntercept(string clueID, System.Action loadScene)
    {
        if (clueID == KnifeClueID && kitchenDoor != null)
        {
            StartCoroutine(WalkToDoorAndExit());
        }
        else
        {
            loadScene();
        }
    }

    IEnumerator WalkToDoorAndExit()
    {
        PlayerController player = PlayerController.Instance;
        if (player == null || kitchenDoor == null) yield break;

        player.CanMove = false;
        kitchenDoor.OpenVisual();

        Collider2D doorCol = kitchenDoor.GetComponent<Collider2D>();
        Vector2 doorTarget = doorCol != null
            ? doorCol.ClosestPoint(player.transform.position)
            : (Vector2)kitchenDoor.transform.position;

if (GridPathfinder.Instance != null)
        {
            List<Vector2> path = GridPathfinder.Instance.FindPath(player.transform.position, doorTarget);
            yield return StartCoroutine(CharacterMover.WalkPath(player.transform, path, characterWalkSpeed));
        }
        else
        {
            yield return StartCoroutine(CharacterMover.Walk(player.transform, doorTarget, characterWalkSpeed));
        }

        kitchenDoor.Interact();
    }
}
