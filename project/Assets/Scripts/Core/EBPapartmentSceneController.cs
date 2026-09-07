using UnityEngine;

public class EBPapartmentSceneController : MonoBehaviour
{
    [Header("Scene Navigation")]

    [Tooltip("Name of the scene to load when the player chooses to leave.")]
    public string nextSceneName;

    [Tooltip("Optional flag that must be set before the player can leave.")]
    public string requiredFlagToLeave;

    public bool CanLeave()
    {
        if (string.IsNullOrEmpty(requiredFlagToLeave))
        {
            return true;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[EBPapartmentSceneController] GameManager.Instance is null.");
            return false;
        }

        return GameManager.Instance.GetFlag(requiredFlagToLeave);
    }

    public void GoToNextScene()
    {
        if (!CanLeave())
        {
            Debug.Log("[EBPapartmentSceneController] The player cannot leave yet.");
            return;
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[EBPapartmentSceneController] No next scene has been specified.");
            return;
        }

        GameManager.Instance?.LoadScene(nextSceneName);
    }
}
