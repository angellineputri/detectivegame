using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenSceneController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] ClueOutcomeTable kitchenOutcomeTable;

    [Header("Head Chef Walk")]
    [Tooltip("The HeadChef NPC Transform in this Kitchen scene.")]
    [SerializeField] Transform headChef;

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

    void Start()
    {
        if (BagUI.Instance != null)
        {
            BagUI.Instance.SetOutcomeTable(kitchenOutcomeTable);
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
                if (GameOverScreen.Instance != null)
                {
                    GameOverScreen.Instance.Show();
                }
            }
            else if (headChef != null && PlayerController.Instance != null)
            {
                StartCoroutine(WalkPlayerToHeadChef(onContinue));
            }
            else
            {
                if (onContinue != null)
                {
                    onContinue();
                }
            }
        }
        else
        {
            if (onContinue != null)
            {
                onContinue();
            }
        }
    }

    IEnumerator WalkPlayerToHeadChef(System.Action onArrived)
    {
        PlayerController player = PlayerController.Instance;
        player.CanMove = false;

        Vector3 playerPos = player.transform.position;
        Vector3 chefPos = headChef.position;
        Vector3 direction = (chefPos - playerPos).normalized;
        Vector3 destination = chefPos - direction * 1f;
        destination.z = playerPos.z;

        if (GridPathfinder.Instance != null)
        {
            List<Vector2> path = GridPathfinder.Instance.FindPath(playerPos, destination);
            yield return StartCoroutine(CharacterMover.WalkPath(player.transform, path, characterWalkSpeed));
        }
        else
        {
            yield return StartCoroutine(CharacterMover.Walk(player.transform, destination, characterWalkSpeed));
        }

        Vector2 playerToChef = ((Vector2)headChef.position - (Vector2)player.transform.position).normalized;
        player.FaceDirection(playerToChef);

        Animator chefAnimator = headChef.GetComponent<Animator>();
        if (chefAnimator != null)
        {
            Vector2 chefToPlayer = -playerToChef;
            chefAnimator.SetFloat("MoveX", chefToPlayer.x);
            chefAnimator.SetFloat("MoveY", chefToPlayer.y);
            chefAnimator.SetFloat("Speed", 0f);
        }

        if (onArrived != null)
        {
            onArrived();
        }
    }
}
