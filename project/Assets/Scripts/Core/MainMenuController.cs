using UnityEngine;

// Attach to any persistent GameObject in MainMenu.unity (e.g. the Canvas root).
//
// WHY THIS EXISTS: Unity serializes button OnClick() references to a specific
// GameObject instance. If that instance is destroyed on scene reload (which happens
// to the GameManager duplicate in Awake), the reference goes dead and the button
// silently does nothing. This script lives in the MainMenu hierarchy so it IS
// recreated on reload — and it looks up GameManager.Instance fresh at call time
// rather than holding a stale serialized reference.
//
// WIRING: In the Inspector, point every MainMenu button's OnClick() at this
// component's methods, NOT at the GameManager GameObject directly.
public class MainMenuController : MonoBehaviour
{
    public void OnStartClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[MainMenuController] GameManager.Instance is null — cannot load SelectCaseScreen.");
            return;
        }
        GameManager.Instance.LoadScene("SelectCaseScreen");
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
