using System.Diagnostics;
using UnityEngine;

public class apartmentSceneController : MonoBehaviour
{
    [Header("Scene Navigation")]

    [Tooltip("Name of the scene to load when the player chooses to leave the apartment.")]
    public string nextSceneName;

    [Tooltip("Optional flag that must be set before the player can leave.")]
    public string requiredFlagToLeave;

    /// <summary>
    /// Checks whether the player is allowed to leave the apartment.
    /// </summary>
    public bool CanLeave()
    {
        // If no flag is required, the player can leave at any time.
        if (string.IsNullOrEmpty(requiredFlagToLeave))
        {
            return true;
        }

        // Make sure GameManager exists.
        if (GameManager.Instance == null)
        {
            UnityEngine.Debug.LogWarning(
                "[ApartmentSceneController] GameManager.Instance is null."
            );

            return false;
        }

        return GameManager.Instance.GetFlag(requiredFlagToLeave);
    }

    /// <summary>
    /// Loads the next scene.
    /// </summary>
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
