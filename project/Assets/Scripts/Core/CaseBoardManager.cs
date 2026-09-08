using System;
using UnityEngine;

public class CaseBoardManager : MonoBehaviour
{
    public static CaseBoardManager Instance { get; private set; }

    [Header("Ex-GF Chain — display names for board chips")]
    [SerializeField] CaseChainNames _exGfChain;

    [Header("Assistant Chain — display names for board chips")]
    [SerializeField] CaseChainNames _assistantChain;

    [Header("Culprit name shown on the merged final card")]
    [SerializeField] string _culpritName = "???";

    public PathStage exGfStage      { get; private set; }
    public PathStage assistantStage { get; private set; }
    public bool      p3Unlocked     { get; private set; }
    public bool      p3Done         { get; private set; }

    public int stageA     => (int)exGfStage;
    public int stageB     => (int)assistantStage;
    public int stageFinal => p3Done ? 2 : p3Unlocked ? 1 : 0;

    public CaseChainNames exGfChain      => _exGfChain;
    public CaseChainNames assistantChain => _assistantChain;
    public string         culpritName    => string.IsNullOrEmpty(_culpritName) ? "???" : _culpritName;

    public event Action OnBoardChanged;

    public event Action OnDeadEnd;

    public event Action OnP3Unlocked;

    const string K_ExGf_P1Pin    = "board_exgf_p1_pinned";
    const string K_ExGf_P1Arr    = "board_exgf_p1_arrested";
    const string K_ExGf_P2Pin    = "board_exgf_p2_pinned";
    const string K_ExGf_P2Arr    = "board_exgf_p2_arrested";
    const string K_ExGf_P2Court  = "board_exgf_p2_court";
    const string K_Asst_P1Pin    = "board_asst_p1_pinned";
    const string K_Asst_P1Arr    = "board_asst_p1_arrested";
    const string K_Asst_P2Pin    = "board_asst_p2_pinned";
    const string K_Asst_P2Arr    = "board_asst_p2_arrested";
    const string K_Asst_P2Court  = "board_asst_p2_court";
    const string K_P3Unlocked    = "p3_unlocked";
    const string K_P3Done        = "board_p3_done";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() => RestoreFromFlags();

    public void ExGf_P1_Pin()
    {
        if (exGfStage >= PathStage.P1_SuspectPinned) return;
        exGfStage = PathStage.P1_SuspectPinned;
        GameManager.Instance?.SetFlag(K_ExGf_P1Pin, true);
        OnBoardChanged?.Invoke();
    }

    public void ExGf_P1_Arrest()
    {
        if (exGfStage >= PathStage.P1_Arrested) return;
        exGfStage = PathStage.P1_Arrested;
        GameManager.Instance?.SetFlag(K_ExGf_P1Arr, true);
        OnBoardChanged?.Invoke();
    }

    public void ExGf_P2_Pin()
    {
        if (exGfStage >= PathStage.P2_SuspectPinned) return;
        exGfStage = PathStage.P2_SuspectPinned;
        GameManager.Instance?.SetFlag(K_ExGf_P2Pin, true);
        OnBoardChanged?.Invoke();
    }

    public void ExGf_P2_Arrest()
    {
        if (exGfStage >= PathStage.P2_Arrested) return;
        exGfStage = PathStage.P2_Arrested;
        GameManager.Instance?.SetFlag(K_ExGf_P2Arr, true);
        OnBoardChanged?.Invoke();
    }

    public void ExGf_P2_CourtDone()
    {
        if (exGfStage >= PathStage.P2_CourtDone) return;
        exGfStage = PathStage.P2_CourtDone;
        GameManager.Instance?.SetFlag(K_ExGf_P2Court, true);
        OnBoardChanged?.Invoke();
        CheckUnlock();
    }

    public void Assistant_P1_Pin()
    {
        if (assistantStage >= PathStage.P1_SuspectPinned) return;
        assistantStage = PathStage.P1_SuspectPinned;
        GameManager.Instance?.SetFlag(K_Asst_P1Pin, true);
        OnBoardChanged?.Invoke();
    }

    public void Assistant_P1_Arrest()
    {
        if (assistantStage >= PathStage.P1_Arrested) return;
        assistantStage = PathStage.P1_Arrested;
        GameManager.Instance?.SetFlag(K_Asst_P1Arr, true);
        OnBoardChanged?.Invoke();
    }

    public void Assistant_P2_Pin()
    {
        if (assistantStage >= PathStage.P2_SuspectPinned) return;
        assistantStage = PathStage.P2_SuspectPinned;
        GameManager.Instance?.SetFlag(K_Asst_P2Pin, true);
        OnBoardChanged?.Invoke();
    }

    public void Assistant_P2_Arrest()
    {
        if (assistantStage >= PathStage.P2_Arrested) return;
        assistantStage = PathStage.P2_Arrested;
        GameManager.Instance?.SetFlag(K_Asst_P2Arr, true);
        OnBoardChanged?.Invoke();
    }

    public void Assistant_P2_CourtDone()
    {
        if (assistantStage >= PathStage.P2_CourtDone) return;
        assistantStage = PathStage.P2_CourtDone;
        GameManager.Instance?.SetFlag(K_Asst_P2Court, true);
        OnBoardChanged?.Invoke();
        CheckUnlock();
    }

    public void MarkP3Done()
    {
        if (p3Done) return;
        p3Done = true;
        GameManager.Instance?.SetFlag(K_P3Done, true);
        OnBoardChanged?.Invoke();
    }

    void CheckUnlock()
    {
        bool exGfDone = exGfStage  >= PathStage.P2_CourtDone;
        bool asstDone = assistantStage >= PathStage.P2_CourtDone;

        if (exGfDone && asstDone)
        {
            if (p3Unlocked) return;
            p3Unlocked = true;
            GameManager.Instance?.SetFlag(K_P3Unlocked, true);
            OnBoardChanged?.Invoke();
            OnP3Unlocked?.Invoke();
            Debug.Log("[CaseBoardManager] P3 unlocked — both branches at P2_CourtDone.");
        }
        else
        {
            OnDeadEnd?.Invoke();
            Debug.Log($"[CaseBoardManager] Dead end — exGf={exGfStage}, assistant={assistantStage}");
        }
    }

    void RestoreFromFlags()
    {
        if (GameManager.Instance == null) return;
        exGfStage      = ReconstructStage(K_ExGf_P1Pin, K_ExGf_P1Arr, K_ExGf_P2Pin, K_ExGf_P2Arr, K_ExGf_P2Court);
        assistantStage = ReconstructStage(K_Asst_P1Pin, K_Asst_P1Arr, K_Asst_P2Pin, K_Asst_P2Arr, K_Asst_P2Court);
        p3Unlocked     = GameManager.Instance.GetFlag(K_P3Unlocked);
        p3Done         = GameManager.Instance.GetFlag(K_P3Done);
        Debug.Log($"[CaseBoardManager] Restored — exGf={exGfStage}, assistant={assistantStage}, stageFinal={stageFinal}");
    }

    PathStage ReconstructStage(string p1Pin, string p1Arr, string p2Pin, string p2Arr, string p2Court)
    {
        var gm = GameManager.Instance;
        if (gm.GetFlag(p2Court)) return PathStage.P2_CourtDone;
        if (gm.GetFlag(p2Arr))   return PathStage.P2_Arrested;
        if (gm.GetFlag(p2Pin))   return PathStage.P2_SuspectPinned;
        if (gm.GetFlag(p1Arr))   return PathStage.P1_Arrested;
        if (gm.GetFlag(p1Pin))   return PathStage.P1_SuspectPinned;
        return PathStage.NotStarted;
    }
}
