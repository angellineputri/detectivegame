using System.Collections;
using UnityEngine;

public class ExGFBedroomSceneController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] ClueOutcomeTable bedroomOutcomeTable;

    [Header("Marcus Commentary")]
    [Tooltip("Played when the player uses Vivian's Address while already in this scene.")]
    [SerializeField] DialogueData marcusAlreadyHereDialogue;

    void Start()
    {
        BagUI.Instance?.SetOutcomeTable(bedroomOutcomeTable);
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

    void HandleSceneLoadIntercept(string clueID, System.Action proceedWithLoad)
    {
        if (clueID == "paper_vivian_address")
        {
            StartCoroutine(PlayAlreadyHere());
        }
        else
        {
            proceedWithLoad?.Invoke();
        }
    }

    IEnumerator PlayAlreadyHere()
    {
        if (marcusAlreadyHereDialogue != null && DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(marcusAlreadyHereDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = true;
        }
    }
}
