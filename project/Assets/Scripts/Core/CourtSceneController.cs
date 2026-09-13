using System.Collections;
using UnityEngine;

public class CourtSceneController : MonoBehaviour
{
    [SerializeField] DialogueData marcusClosingDialogue;
    [SerializeField] DialogueData headChefRevealDialogue;

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
            yield break;
        }

        CaseBoardManager.Instance?.ExGf_P1_Arrest();

        yield return new WaitUntil(() => CaseBoardUI.Instance == null || !CaseBoardUI.Instance.IsBoardVisible);
        yield return new WaitForSeconds(0.5f);

        if (marcusClosingDialogue != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(marcusClosingDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }
        else
        {
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
        }

        GameManager.Instance?.AdvancePlaythrough();
        GameManager.Instance?.LoadScene("ExGF_ChefRoom_P1");
    }
}
