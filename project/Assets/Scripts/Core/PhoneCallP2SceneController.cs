using System.Collections;
using UnityEngine;

public class PhoneCallP2SceneController : MonoBehaviour
{
    [Header("Dialogues")]
    [SerializeField] DialogueData chiefInspectorDialogue;
    [SerializeField] DialogueData thinkingDialogue;

    [Header("Timing")]
    [SerializeField] float preDialogueDelay = 0.5f;

    const string PhoneCallCompletedFlag = "p2_phoneCallCompleted";

    void Start()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }

        StartCoroutine(PhoneCallSequence());
    }

    IEnumerator PhoneCallSequence()
    {
        yield return new WaitForSeconds(preDialogueDelay);

        if (chiefInspectorDialogue != null && DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(chiefInspectorDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        if (thinkingDialogue != null && DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(thinkingDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        GameManager.Instance?.SetFlag(PhoneCallCompletedFlag, true);
        GameManager.Instance?.LoadScene("ExGF_Diner_P2");
    }
}
