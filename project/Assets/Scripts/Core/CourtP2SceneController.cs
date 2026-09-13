using System.Collections;
using UnityEngine;

public class CourtP2SceneController : MonoBehaviour
{
    [SerializeField] DialogueData chiefInspectorDialogue;
    [SerializeField] DialogueData headChefCallDialogue;

    [Header("Post-Call — Player Thinking")]
    [Tooltip("Plays after the head chef phone call, before the case board dead-end ??? appears.")]
    [SerializeField] DialogueData thinkingDialogue;
    [Tooltip("Delay before the thinking dialogue starts (after the phone call ends).")]
    [SerializeField] float pauseBeforeThinking = 0.5f;

    [Header("Scene Navigation")]
    [Tooltip("Loaded when the assistant path has NOT been played yet.")]
    [SerializeField] string revisitSceneName = "VictimApartmentRevisitForAssistant";
    [Tooltip("Loaded when the assistant path has already been played (both paths done).")]
    [SerializeField] string finalSceneName = "VictimApartmentRevisitFinal";

    [SerializeField] float pauseBetweenBeats = 1f;

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
            Debug.LogError("[CourtP2SceneController] DialogueRunner.Instance is null. Is the PersistentSystems prefab in this scene?");
            yield break;
        }

        CaseBoardManager.Instance?.ExGf_P2_Arrest();

        yield return new WaitUntil(() => CaseBoardUI.Instance == null || !CaseBoardUI.Instance.IsBoardVisible);
        yield return new WaitForSeconds(0.5f);

        if (chiefInspectorDialogue != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(chiefInspectorDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }
        else
        {
            Debug.LogWarning("[CourtP2SceneController] chiefInspectorDialogue is not assigned.");
        }

        yield return new WaitForSeconds(pauseBetweenBeats);

        if (headChefCallDialogue != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(headChefCallDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }
        else
        {
            Debug.LogWarning("[CourtP2SceneController] headChefCallDialogue is not assigned.");
        }

        yield return new WaitForSeconds(pauseBeforeThinking);

        if (thinkingDialogue != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(thinkingDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }
        else
        {
            Debug.LogWarning("[CourtP2SceneController] thinkingDialogue is not assigned.");
        }

        CaseBoardManager.Instance?.ExGf_P2_CourtDone();

        yield return new WaitUntil(() => CaseBoardUI.Instance == null || !CaseBoardUI.Instance.IsBoardVisible);
        yield return new WaitForSeconds(0.5f);

        bool assistantNotStarted = CaseBoardManager.Instance == null
            || CaseBoardManager.Instance.assistantStage == PathStage.NotStarted;

        string nextScene = assistantNotStarted ? revisitSceneName : finalSceneName;
        if (!string.IsNullOrEmpty(nextScene))
        {
            GameManager.Instance?.LoadScene(nextScene);
        }
        else
        {
            Debug.LogWarning("[CourtP2SceneController] next scene name is empty.");
        }
    }
}
