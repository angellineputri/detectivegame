using System.Collections;
using UnityEngine;

public class CourtP2SceneController : MonoBehaviour
{
    [SerializeField] DialogueData chiefInspectorDialogue;
    [SerializeField] DialogueData headChefCallDialogue;

    [Header("Post-Call — Assistant Path Not Yet Visited")]
    [Tooltip("Plays after the case board dead-end ??? closes (only when assistant path not yet started).")]
    [SerializeField] DialogueData thinkingDialogue;
    [Tooltip("Scene to load after the thinking dialogue when assistant path has not been visited.")]
    [SerializeField] string revisitSceneName = "VictimApartmentRevisitForAssistant";

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

        CaseBoardManager.Instance?.ExGf_P2_CourtDone();

        yield return new WaitUntil(() => CaseBoardUI.Instance == null || !CaseBoardUI.Instance.IsBoardVisible);

        bool assistantNotStarted = CaseBoardManager.Instance == null
            || CaseBoardManager.Instance.assistantStage == PathStage.NotStarted;

        if (assistantNotStarted)
        {
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

            if (!string.IsNullOrEmpty(revisitSceneName))
            {
                GameManager.Instance?.LoadScene(revisitSceneName);
            }
        }
    }
}
