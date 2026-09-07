using UnityEngine;

public class officeSceneController : MonoBehaviour
{
    [Header("Scene Navigation")]

    [Tooltip("Name of the scene to load when the player chooses to leave the office.")]
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
            Debug.LogWarning("[OfficeSceneController] GameManager.Instance is null.");
            return false;
        }

        return GameManager.Instance.GetFlag(requiredFlagToLeave);
    }

    public void GoToNextScene()
    {
        if (!CanLeave())
        {
            Debug.Log("[OfficeSceneController] The player cannot leave the office yet.");
            return;
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[OfficeSceneController] No next scene has been specified.");
            return;
        }

        GameManager.Instance?.LoadScene(nextSceneName);
    }
}
