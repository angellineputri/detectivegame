using System.Collections;
using UnityEngine;

public class CourtP2SceneController : MonoBehaviour
{
    [SerializeField] DialogueData chiefInspectorDialogue;
    [SerializeField] DialogueData headChefCallDialogue;

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
    }
}
