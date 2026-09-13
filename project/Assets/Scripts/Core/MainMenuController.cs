using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene names")]
    [SerializeField] string caseBriefingScene = "SelectCaseScreen";
    [SerializeField] string endingsScene      = "Endings";
    [SerializeField] string settingsScene     = "Settings";
    [SerializeField] string creditsScene      = "Credits";

    [Header("Buttons (wired by setup tool)")]
    [SerializeField] Button startButton;
    [SerializeField] Button endingsButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button creditsButton;

    void Start()
    {
        startButton?.onClick.AddListener(OnStartClicked);
        endingsButton?.onClick.AddListener(OnEndings);
        settingsButton?.onClick.AddListener(OnSettings);
        creditsButton?.onClick.AddListener(OnCredits);
    }

    public void OnStartClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        BagUI.Instance?.ClearBag();
        Load(caseBriefingScene);
    }
    public void OnEndings()  { AudioManager.Instance?.PlayButtonClick(); Load(endingsScene); }
    public void OnSettings() { AudioManager.Instance?.PlayButtonClick(); Load(settingsScene); }
    public void OnCredits()  { AudioManager.Instance?.PlayButtonClick(); Load(creditsScene); }

    public void OnQuitClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Load(string scene)
    {
        if (string.IsNullOrEmpty(scene)) return;
        if (GameManager.Instance != null)
            GameManager.Instance.LoadScene(scene);
        else
            SceneManager.LoadScene(scene);
    }
}
