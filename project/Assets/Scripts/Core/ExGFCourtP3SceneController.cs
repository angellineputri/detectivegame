using System.Collections;
using UnityEngine;

public class ExGFCourtP3SceneController : MonoBehaviour
{
    [Header("Dialogue Beats")]
    [SerializeField] DialogueData encounterDialogue;
    [SerializeField] DialogueData realizationDialogue;
    [SerializeField] DialogueData arrestDialogue;
    [SerializeField] DialogueData labDialogue;
    [SerializeField] float beatDelay = 0.5f;

    [Header("Ending")]
    [SerializeField] string mainMenuScene = "MainMenu";

    void Start()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.CanMove = false;

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        yield return Play(encounterDialogue);
        yield return new WaitForSeconds(beatDelay);

        yield return Play(realizationDialogue);
        yield return new WaitForSeconds(beatDelay);

        yield return Play(arrestDialogue);
        yield return new WaitForSeconds(beatDelay);

        yield return Play(labDialogue);

        CaseBoardManager.Instance?.MarkP3Done();

        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() =>
            CaseBoardUI.Instance == null || !CaseBoardUI.Instance.IsBoardVisible);

        if (!string.IsNullOrEmpty(mainMenuScene))
            GameManager.Instance?.LoadScene(mainMenuScene);
    }

    IEnumerator Play(DialogueData dialogue)
    {
        if (dialogue == null || DialogueRunner.Instance == null) yield break;
        bool done = false;
        DialogueRunner.Instance.Play(dialogue, () => done = true);
        yield return new WaitUntil(() => done);
    }
}
