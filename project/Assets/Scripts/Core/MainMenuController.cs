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

    public void OnStartClicked()  => Load(caseBriefingScene);
    public void OnEndings()       => Load(endingsScene);
    public void OnSettings()      => Load(settingsScene);
    public void OnCredits()       => Load(creditsScene);

    public void OnQuitClicked()
    {
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
