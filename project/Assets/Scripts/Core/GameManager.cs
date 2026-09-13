using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int CurrentPlaythrough { get; private set; } = 1;

    public string SelectedCaseID { get; private set; }

    public event System.Action<string> OnClueAdded;
    public event System.Action OnPlaythroughAdvanced;

    public HashSet<string> FoundClues { get; private set; } = new HashSet<string>();

    private Dictionary<string, bool> _flags = new Dictionary<string, bool>();

    public string LastScene { get; private set; }

    public Vector2 PendingSpawnPosition { get; private set; }
    public bool    HasPendingSpawn      { get; private set; }

    public void SetPendingSpawn(Vector2 position)
    {
        PendingSpawnPosition = position;
        HasPendingSpawn      = true;
    }

    public void ConsumePendingSpawn()
    {
        HasPendingSpawn = false;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectCase(string caseID)
    {
        SelectedCaseID = caseID;
        CurrentPlaythrough = 1;
        FoundClues.Clear();
        _flags.Clear();

        CaseBoardManager.Instance?.ResetForNewCase();
    }

    public void AdvancePlaythrough()
    {
        CurrentPlaythrough++;
        ClearClues();
        OnPlaythroughAdvanced?.Invoke();
    }

    public void SetPlaythrough(int n)
    {
        CurrentPlaythrough = n;
    }

    public void AddClue(string clueID)
    {
        if (FoundClues.Add(clueID))
        {
            OnClueAdded?.Invoke(clueID);
            AudioManager.Instance?.PlayClueAdded();
        }
    }

    public bool HasClue(string clueID)
    {
        return FoundClues.Contains(clueID);
    }

    public void ClearClues()
    {
        FoundClues.Clear();
    }

    public void SetFlag(string key, bool value)
    {
        _flags[key] = value;
    }

    public bool GetFlag(string key)
    {
        bool val;
        _flags.TryGetValue(key, out val);
        return val;
    }

    public void LoadScene(string sceneName)
    {
        LastScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
