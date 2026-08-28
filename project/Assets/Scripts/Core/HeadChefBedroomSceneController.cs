using System.Collections;
using UnityEngine;

public class HeadChefBedroomSceneController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] ClueOutcomeTable bedroomOutcomeTable;

    [Header("Head Chef Walk")]
    [Tooltip("The HeadChef NPC Transform in this scene.")]
    [SerializeField] Transform headChef;

    [Tooltip("How many units above or below HeadChef the player stops.")]
    [SerializeField] float headChefStandOffset = 1f;

    [Tooltip("Walk speed in units per second.")]
    [SerializeField] float characterWalkSpeed = 3f;

    [Header("Scene Obstacles")]
    [Tooltip("Assign all 6 bedroom prop Transforms. The player routes around these during the walk to HeadChef.")]
    [SerializeField] Transform[] sceneObstacles;

    [Tooltip("Avoidance clearance radius around each scene obstacle.")]
    [SerializeField] float obstacleRadius = 0.8f;

    void Start()
    {
        BagUI.Instance?.SetOutcomeTable(bedroomOutcomeTable);
    }

    void OnEnable()
    {
        ItemSelectionUI.PreDialogueWalkHandler = HandlePreDialogueWalk;
    }

    void OnDisable()
    {
        if (ItemSelectionUI.PreDialogueWalkHandler == HandlePreDialogueWalk)
        {
            ItemSelectionUI.PreDialogueWalkHandler = null;
        }
    }

    void HandlePreDialogueWalk(string clueID, System.Action onContinue)
    {
        if (clueID == "chef_computer")
        {
            if (headChef != null && PlayerController.Instance != null)
            {
                StartCoroutine(WalkPlayerToHeadChef(onContinue));
            }
            else
            {
                onContinue?.Invoke();
            }
        }
        else
        {
            onContinue?.Invoke();
        }
    }

    IEnumerator WalkPlayerToHeadChef(System.Action onArrived)
    {
        PlayerController player = PlayerController.Instance;
        player.CanMove = false;

        float playerY = player.transform.position.y;
        float yAbove  = headChef.position.y + headChefStandOffset;
        float yBelow  = headChef.position.y - headChefStandOffset;

        float destY;
        if (Mathf.Abs(playerY - yAbove) <= Mathf.Abs(playerY - yBelow))
        {
            destY = yAbove;
        }
        else
        {
            destY = yBelow;
        }

        Vector3 destination = new Vector3(headChef.position.x, destY, player.transform.position.z);

        Debug.Log("[HeadChefBedroomSceneController] Walking player to HeadChef at " + destination);

        yield return StartCoroutine(CharacterMover.Walk(player.transform, destination, characterWalkSpeed, sceneObstacles, obstacleRadius));

        Debug.Log("[HeadChefBedroomSceneController] Walk complete.");
        onArrived?.Invoke();
    }
}
