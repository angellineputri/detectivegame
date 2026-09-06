using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSelectionUI : MonoBehaviour
{
    public static ItemSelectionUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text promptText;
    [SerializeField] Transform buttonContainer;
    [SerializeField] GameObject buttonPrefab;

    ClueOutcomeTable _table;
    System.Action<string> _callback;
    System.Action _onDecline;
    readonly List<GameObject> _spawnedButtons = new List<GameObject>();

    public static System.Action<string, System.Action> PreDialogueWalkHandler;
    public static System.Action<string, System.Action> SceneLoadInterceptHandler;

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
    }

    public void Show(IEnumerable<string> presentableClues, ClueOutcomeTable table, string prompt = "Which piece of evidence do you present?")
    {
        _table = table;
        _callback = null;
        _onDecline = null;
        promptText.text = prompt;

        ClearButtons();

        bool anyFound = false;
        foreach (string clueID in presentableClues)
        {
            if (!GameManager.Instance.HasClue(clueID)) continue;
            anyFound = true;
            SpawnButton(clueID);
        }

        if (!anyFound)
        {
            GameOverScreen.Instance?.Show();
            return;
        }

        panel.SetActive(true);
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }
    }

    public void ShowWithCallback(IEnumerable<string> presentableClues, System.Action<string> onClueChosen, string prompt = "Which piece of evidence do you present?", string declineLabel = null, System.Action onDecline = null)
    {
        _table = null;
        _callback = onClueChosen;
        _onDecline = onDecline;
        promptText.text = prompt;

        ClearButtons();

        bool anyFound = false;
        foreach (string clueID in presentableClues)
        {
            if (!GameManager.Instance.HasClue(clueID)) continue;
            anyFound = true;
            SpawnButton(clueID);
        }

        if (!string.IsNullOrEmpty(declineLabel))
        {
            GameObject go = Instantiate(buttonPrefab, buttonContainer);
            _spawnedButtons.Add(go);
            TMP_Text lbl = go.GetComponentInChildren<TMP_Text>();
            if (lbl != null)
            {
                lbl.text = declineLabel;
            }
            go.GetComponent<Button>().onClick.AddListener(OnDeclineSelected);
        }

        if (!anyFound && string.IsNullOrEmpty(declineLabel))
        {
            GameOverScreen.Instance?.Show();
            return;
        }

        panel.SetActive(true);
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }
    }

    void SpawnButton(string clueID)
    {
        GameObject go = Instantiate(buttonPrefab, buttonContainer);
        _spawnedButtons.Add(go);

        TMP_Text label = go.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = BagUI.Instance?.GetDisplayName(clueID) ?? clueID;
        }

        go.GetComponent<Button>().onClick.AddListener(() => OnClueSelected(clueID));
    }

    void ClearButtons()
    {
        foreach (GameObject b in _spawnedButtons)
        {
            Destroy(b);
        }
        _spawnedButtons.Clear();
    }

    void OnClueSelected(string clueID)
    {
        panel.SetActive(false);
        ClearButtons();
        _onDecline = null;

        if (_callback != null)
        {
            System.Action<string> cb = _callback;
            _callback = null;
            cb.Invoke(clueID);
            return;
        }

        ClueOutcome outcome = _table?.GetOutcome(clueID);
        if (outcome == null)
        {
            GameOverScreen.Instance?.Show();
            return;
        }

        ExecuteOutcome(outcome);
    }

    void OnDeclineSelected()
    {
        panel.SetActive(false);
        ClearButtons();
        _callback = null;
        System.Action cb = _onDecline;
        _onDecline = null;
        cb?.Invoke();
    }

    public void ExecuteOutcome(ClueOutcome outcome)
    {
        switch (outcome.outcomeType)
        {
            case OutcomeType.ShowPopup:
                UIManager.Instance?.ShowCluePopup(outcome.popupText, null);
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.CanMove = true;
                }
                break;

            case OutcomeType.GameOver:
                GameOverScreen.Instance?.Show();
                break;

            case OutcomeType.LoadScene:
                if (SceneLoadInterceptHandler != null)
                {
                    SceneLoadInterceptHandler.Invoke(outcome.clueID, () => GameManager.Instance?.LoadScene(outcome.targetScene));
                }
                else
                {
                    GameManager.Instance?.LoadScene(outcome.targetScene);
                }
                break;

            case OutcomeType.TriggerDialogue:
                if (PreDialogueWalkHandler != null)
                {
                    PreDialogueWalkHandler.Invoke(outcome.clueID, () => ExecuteTriggerDialogue(outcome));
                }
                else
                {
                    ExecuteTriggerDialogue(outcome);
                }
                break;

            case OutcomeType.SetFlag:
                GameManager.Instance?.SetFlag(outcome.flagKey, outcome.flagValue);
                if (!string.IsNullOrEmpty(outcome.targetScene))
                {
                    GameManager.Instance?.LoadScene(outcome.targetScene);
                }
                else if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.CanMove = true;
                }
                break;

            case OutcomeType.AdvancePlaythrough:
                GameManager.Instance?.AdvancePlaythrough();
                if (!string.IsNullOrEmpty(outcome.targetScene))
                {
                    GameManager.Instance?.LoadScene(outcome.targetScene);
                }
                break;
        }
    }

    void ExecuteTriggerDialogue(ClueOutcome outcome)
    {
        Debug.Log("[ItemSelectionUI] ExecuteTriggerDialogue: clueID='" + outcome.clueID + "'");

        if (outcome.dialogue != null && DialogueRunner.Instance != null)
        {
            DialogueRunner.Instance.Play(outcome.dialogue, () =>
            {
                Debug.Log("[ItemSelectionUI] Dialogue complete for '" + outcome.clueID + "'");

                ItemGrant grant = outcome.itemToGrantAfterDialogue;
                if (!string.IsNullOrEmpty(grant?.objectID))
                {
                    BagUI.Instance?.RegisterClueDisplayName(grant.objectID, grant.displayName);
                    GameManager.Instance?.AddClue(grant.objectID);
                }

                if (!string.IsNullOrEmpty(outcome.targetScene))
                {
                    if (SceneLoadInterceptHandler != null)
                        SceneLoadInterceptHandler.Invoke(outcome.clueID, () => GameManager.Instance?.LoadScene(outcome.targetScene));
                    else
                        GameManager.Instance?.LoadScene(outcome.targetScene);
                }
                else if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.CanMove = true;
                }
            });
        }
        else if (!string.IsNullOrEmpty(outcome.targetScene))
        {
            GameManager.Instance?.LoadScene(outcome.targetScene);
        }
        else
        {
            if (outcome.dialogue == null)
            {
                Debug.LogError("[ItemSelectionUI] TriggerDialogue outcome for '" + outcome.clueID + "' has no DialogueData assigned.");
            }
            if (DialogueRunner.Instance == null)
            {
                Debug.LogError("[ItemSelectionUI] DialogueRunner.Instance is null — is PersistentSystems in the scene?");
            }
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.CanMove = true;
            }
        }
    }
}
