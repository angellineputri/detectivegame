using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] Button retrySceneButton;

    [FormerlySerializedAs("returnToDinerButton")]
    [SerializeField] Button restartGameButton;

    [FormerlySerializedAs("restartGameButton")]
    [SerializeField] Button mainMenuButton;

    [Header("Back to Main Menu confirmation")]
    [SerializeField] GameObject confirmPanel;
    [SerializeField] Button confirmYesButton;
    [SerializeField] Button confirmNoButton;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        panel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);

        retrySceneButton?.onClick.AddListener(OnRetryScene);
        restartGameButton?.onClick.AddListener(OnRestartGame);
        mainMenuButton?.onClick.AddListener(OnBackToMainMenu);
        confirmYesButton?.onClick.AddListener(OnConfirmMainMenuYes);
        confirmNoButton?.onClick.AddListener(OnConfirmMainMenuNo);
    }

    public void Show()
    {
        panel.SetActive(true);
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }
    }

    void OnRetryScene()
    {
        panel.SetActive(false);
        ClearAllProgress();
        GameManager.Instance?.ReloadCurrentScene();
    }

    void OnRestartGame()
    {
        panel.SetActive(false);
        ClearAllProgress();
        GameManager.Instance?.LoadScene("VictimApartment");
    }

    void OnBackToMainMenu()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
        }
        else
        {

            OnConfirmMainMenuYes();
        }
    }

    void OnConfirmMainMenuYes()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
        panel.SetActive(false);
        ClearAllProgress();
        GameManager.Instance?.LoadScene("MainMenu");
    }

    void OnConfirmMainMenuNo()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    void ClearAllProgress()
    {
        GameManager.Instance?.ClearClues();
        BagUI.Instance?.ClearBag();
        GameManager.Instance?.SetFlag("allDinerCluesFound", false);
        GameManager.Instance?.SetFlag("hasSpokenToExGF", false);
        GameManager.Instance?.SetFlag("hasSpokenToHeadChef", false);
        GameManager.Instance?.SetFlag("hasSpokenToVivianBedroom", false);
        GameManager.Instance?.SetFlag("assistant_keys_given",false);
    }
}
