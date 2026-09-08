using UnityEngine;

public class apartment_2ndSceneController : MonoBehaviour
{
    [Header("Scene Navigation")]

    [Tooltip("Name of the scene to load when the player chooses to leave the apartment.")]
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
            UnityEngine.Debug.LogWarning(
                "[ApartmentSceneController] GameManager.Instance is null."
            );

            return false;
        }

        return GameManager.Instance.GetFlag(requiredFlagToLeave);
    }

    public void GoToNextScene()
    {
        if (!CanLeave())
        {
            UnityEngine.Debug.Log(
                "[ApartmentSceneController] The player cannot leave the apartment yet."
            );

            return;
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.Debug.LogWarning(
                "[ApartmentSceneController] No next scene has been specified."
            );

            return;
        }

        GameManager.Instance?.LoadScene(nextSceneName);
    }
}
