using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Playthrough 2 — kitchen transition scene.
// The player finds the bagged knife and submits it to the lab via the outcome table
// (TriggerDialogue → grants lab_results). Instead of loading ExGF_Diner_P2 directly,
// the door auto-opens, the player auto-walks to it, and the door loads the scene.
public class KitchenP2SceneController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] ClueOutcomeTable kitchenP2OutcomeTable;

    [Header("Door")]
    [SerializeField] SilentDoor kitchenDoor;

    [Header("Movement")]
    [SerializeField] float characterWalkSpeed = 3f;

    void Start()
    {
        BagUI.Instance?.SetOutcomeTable(kitchenP2OutcomeTable);

        foreach (Interactable i in FindObjectsOfType<Interactable>())
        {
            i.RefreshActiveState();
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

    void HandleSceneLoadIntercept(string clueID, System.Action loadScene)
    {
        if (clueID == "bloody_knife_bagged" && kitchenDoor != null)
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

        if (GridPathfinder.Instance != null)
        {
            List<Vector2> path = GridPathfinder.Instance.FindPath(player.transform.position, kitchenDoor.transform.position);
            yield return StartCoroutine(CharacterMover.WalkPath(player.transform, path, characterWalkSpeed));
        }
        else
        {
            yield return StartCoroutine(CharacterMover.Walk(player.transform, kitchenDoor.transform.position, characterWalkSpeed));
        }

        kitchenDoor.Interact();
    }
}
