using System.Collections;
using UnityEngine;

public class CourtSceneController : MonoBehaviour
{
    [SerializeField] DialogueData marcusClosingDialogue;
    [SerializeField] DialogueData headChefRevealDialogue;

    [Tooltip("Seconds of silence between the Marcus/Player exchange and the Head Chef reveal.")]
    [SerializeField] float pauseBeforeReveal = 1f;

    void Start()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }

        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        if (DialogueRunner.Instance == null)
        {
            Debug.LogError("[CourtSceneController] DialogueRunner.Instance is null. Is the PersistentSystems prefab in this scene?");
            yield break;
        }

        if (marcusClosingDialogue != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(marcusClosingDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }
        else
        {
            Debug.LogWarning("[CourtSceneController] marcusClosingDialogue is not assigned.");
        }

        yield return new WaitForSeconds(pauseBeforeReveal);

        if (headChefRevealDialogue != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(headChefRevealDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }
        else
        {
            Debug.LogWarning("[CourtSceneController] headChefRevealDialogue is not assigned.");
        }

        GameManager.Instance?.AdvancePlaythrough();
        GameManager.Instance?.LoadScene("HeadChefRoom");
    }
}
