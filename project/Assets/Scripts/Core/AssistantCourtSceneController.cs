using System.Collections;
using UnityEngine;

public class AssistantCourtSceneController : MonoBehaviour
{
    [Header("Dialogue — only when ex-gf path not yet done")]
    [SerializeField] DialogueData chiefDialogue;

    [Header("Dialogue — always plays (after chief if ex-gf not done, directly if done)")]
    [SerializeField] DialogueData playerMonologue;

    [Header("Timing")]
    [SerializeField] float pauseBeforeMonologue = 0.5f;

    [Header("Scene Navigation")]
    [SerializeField] string sceneIfExGfNotDone  = "VictimApartmentRevisitForExGF";
    [SerializeField] string sceneIfExGfDone     = "VictimApartmentRevisitFinal";

    void Start()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.CanMove = false;

        StartCoroutine(PlayCourtSequence());
    }

    IEnumerator PlayCourtSequence()
    {

        CaseBoardManager.Instance?.Assistant_P1_Arrest();

        yield return new WaitUntil(() =>
            CaseBoardUI.Instance == null || !CaseBoardUI.Instance.IsBoardVisible);
        yield return new WaitForSeconds(0.5f);

        bool exGfDone = CaseBoardManager.Instance != null &&
                        CaseBoardManager.Instance.exGfStage >= PathStage.P1_Arrested;

        if (!exGfDone)
        {
            yield return PlayDialogue(chiefDialogue);
            yield return new WaitForSeconds(pauseBeforeMonologue);
        }

        yield return PlayDialogue(playerMonologue);

        CaseBoardManager.Instance?.Assistant_P1_CourtDone();

        yield return new WaitUntil(() =>
            CaseBoardUI.Instance == null || !CaseBoardUI.Instance.IsBoardVisible);
        yield return new WaitForSeconds(0.5f);

        string nextScene = exGfDone ? sceneIfExGfDone : sceneIfExGfNotDone;
        GameManager.Instance?.LoadScene(nextScene);
    }

    IEnumerator PlayDialogue(DialogueData dialogue)
    {
        if (dialogue == null || DialogueRunner.Instance == null) yield break;

        bool done = false;
        DialogueRunner.Instance.Play(dialogue, () => done = true);
        yield return new WaitUntil(() => done);
    }
}
