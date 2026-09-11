using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] Button retrySceneButton;
    [SerializeField] Button returnToDinerButton;
    [SerializeField] Button restartGameButton;

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
        retrySceneButton?.onClick.AddListener(OnRetryScene);
        returnToDinerButton?.onClick.AddListener(OnReturnToDiner);
        restartGameButton?.onClick.AddListener(OnRestartGame);
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

    void OnReturnToDiner()
    {
        panel.SetActive(false);
        ClearAllProgress();
        GameManager.Instance?.LoadScene("Diner");
    }

    void OnRestartGame()
    {
        panel.SetActive(false);
        ClearAllProgress();
        GameManager.Instance?.LoadScene("Diner");
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
