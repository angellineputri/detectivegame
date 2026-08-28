using System.Collections;
using UnityEngine;

public class KitchenSceneController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] ClueOutcomeTable kitchenOutcomeTable;

    [Header("Head Chef Walk")]
    [Tooltip("The HeadChef NPC Transform in this Kitchen scene.")]
    [SerializeField] Transform headChef;

    [Tooltip("How many units above or below HeadChef the player stops.")]
    [SerializeField] float headChefStandOffset = 1f;

    [Tooltip("Walk speed in units per second.")]
    [SerializeField] float characterWalkSpeed = 3f;

    [Header("Marcus Commentary")]
    [Tooltip("Dialogue played after the player picks up the Bloody Knife.")]
    [SerializeField] DialogueData marcusBloodyKnifeDialogue;

    [Tooltip("Dialogue played after the player picks up the Knife Rack clue.")]
    [SerializeField] DialogueData marcusKnifeRackDialogue;

    [Tooltip("Seconds to pause between pickup popup closing and Marcus's line starting.")]
    [SerializeField] float marcusCommentaryDelay = 1f;

    [Tooltip("Title shown in the forced bag panel after Marcus's commentary closes.")]
    [SerializeField] string kitchenDecisionTitle = "Make Your Move.";

    [Header("Scene Obstacles")]
    [Tooltip("Scene objects the player should route around when walking to HeadChef.")]
    [SerializeField] Transform[] sceneObstacles;

    [Tooltip("Avoidance clearance radius around each scene obstacle.")]
    [SerializeField] float obstacleRadius = 0.8f;

    void Start()
    {
        BagUI.Instance?.SetOutcomeTable(kitchenOutcomeTable);
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
        ItemSelectionUI.PreDialogueWalkHandler = HandlePreDialogueWalk;
    }

    void OnDisable()
    {
        if (ItemSelectionUI.PreDialogueWalkHandler == HandlePreDialogueWalk)
        {
            ItemSelectionUI.PreDialogueWalkHandler = null;
        }
    }

    void OnClueAdded(string clueID)
    {
        DialogueData marcus = null;

        if (clueID == "kitchen_walkin_fridge")
        {
            marcus = marcusBloodyKnifeDialogue;
        }
        else if (clueID == "kitchen_knife_rack")
        {
            marcus = marcusKnifeRackDialogue;
        }

        if (marcus != null && DialogueRunner.Instance != null)
        {
            StartCoroutine(PlayMarcusCommentary(marcus));
        }
    }

    IEnumerator PlayMarcusCommentary(DialogueData dialogue)
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }

        yield return new WaitForSeconds(marcusCommentaryDelay);

        bool done = false;
        DialogueRunner.Instance.Play(dialogue, () => done = true);
        yield return new WaitUntil(() => done);

        if (BagUI.Instance != null)
        {
            BagUI.Instance.ForceOpenForDecision(kitchenDecisionTitle, null);
        }
        else if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = true;
        }
    }

    void HandlePreDialogueWalk(string clueID, System.Action onContinue)
    {
        if (clueID == "kitchen_walkin_fridge")
        {
            bool hasRackClue = GameManager.Instance != null && GameManager.Instance.HasClue("kitchen_knife_rack");

            if (!hasRackClue)
            {
                GameOverScreen.Instance?.Show();
            }
            else if (headChef != null && PlayerController.Instance != null)
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

        Debug.Log("[KitchenSceneController] Walking player to HeadChef at " + destination);

        yield return StartCoroutine(CharacterMover.Walk(player.transform, destination, characterWalkSpeed, sceneObstacles, obstacleRadius));

        Debug.Log("[KitchenSceneController] Walk complete.");
        onArrived?.Invoke();
    }
}
