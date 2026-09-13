using System.Collections;
using UnityEngine;

public class ExGFExRoomP3SceneController : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Every entry should be GameOver — any 'make a move' ends the game.")]
    [SerializeField] ClueOutcomeTable gameOverTable;

    [Header("Photo Frame Discovery")]
    [SerializeField] string photoFrameClueID = "exgf_photoframe_ci";
    [SerializeField] DialogueData photoFrameThinking;
    [SerializeField] float pauseBeforeThinking = 0.5f;
    [SerializeField] string nextSceneName = "ExGF_Court_P3";

    bool _advancing;

    void Start()
    {
        BagUI.Instance?.SetOutcomeTable(gameOverTable);
        BagUI.Instance?.SetHudVisible(true);

        foreach (Interactable i in FindObjectsOfType<Interactable>())
            i.RefreshActiveState();
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnClueAdded += OnClueAdded;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnClueAdded -= OnClueAdded;
    }

    void OnClueAdded(string clueID)
    {
        if (_advancing || clueID != photoFrameClueID) return;
        _advancing = true;
        StartCoroutine(AdvanceSequence());
    }

    IEnumerator AdvanceSequence()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.CanMove = false;

        yield return new WaitUntil(() =>
            (UIManager.Instance == null || !UIManager.Instance.IsPopupVisible) &&
            (DialogueRunner.Instance == null || !DialogueRunner.Instance.IsPlaying));

        yield return new WaitForSeconds(pauseBeforeThinking);

        if (photoFrameThinking != null && DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(photoFrameThinking, () => done = true);
            yield return new WaitUntil(() => done);
        }

        if (!string.IsNullOrEmpty(nextSceneName))
            GameManager.Instance?.LoadScene(nextSceneName);
        else
            Debug.LogWarning("[ExGFExRoomP3SceneController] nextSceneName is empty.");
    }
}
