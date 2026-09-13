using System;
using UnityEngine;

public class CaseBoardManager : MonoBehaviour
{
    public static CaseBoardManager Instance { get; private set; }

    [SerializeField] CaseChainNames _exGfChain;
    [SerializeField] CaseChainNames _assistantChain;
    [SerializeField] string _culpritName = "???";

    public PathStage exGfStage { get; private set; }
    public PathStage assistantStage { get; private set; }
    public bool exGfP1CourtDone { get; private set; }
    public bool assistantP1CourtDone { get; private set; }
    public bool p3Done { get; private set; }

    public int stageA => (int)exGfStage;
    public int stageB => (int)assistantStage;

    public CaseChainNames exGfChain => _exGfChain;
    public CaseChainNames assistantChain => _assistantChain;
    public string culpritName => string.IsNullOrEmpty(_culpritName) ? "???" : _culpritName;

    public event Action OnBoardChanged;

    const string K_ExGf_P1Pin = "board_exgf_p1_pinned";
    const string K_ExGf_P1Arr = "board_exgf_p1_arrested";
    const string K_ExGf_P1Court = "board_exgf_p1_court";
    const string K_ExGf_P2Pin = "board_exgf_p2_pinned";
    const string K_ExGf_P2Arr = "board_exgf_p2_arrested";
    const string K_ExGf_P2Court = "board_exgf_p2_court";
    const string K_Asst_P1Pin = "board_asst_p1_pinned";
    const string K_Asst_P1Arr = "board_asst_p1_arrested";
    const string K_Asst_P1Court = "board_asst_p1_court";
    const string K_P3Done = "board_p3_done";

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

    void Start()
    {
        RestoreFromFlags();
    }

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

    public void ExGf_P1_CourtDone()
    {
        if (exGfP1CourtDone) return;
        exGfP1CourtDone = true;
        GameManager.Instance?.SetFlag(K_ExGf_P1Court, true);
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

    public void Assistant_P1_CourtDone()
    {
        if (assistantP1CourtDone) return;
        assistantP1CourtDone = true;
        GameManager.Instance?.SetFlag(K_Asst_P1Court, true);
        OnBoardChanged?.Invoke();
    }

    public void MarkP3Done()
    {
        if (p3Done) return;
        p3Done = true;
        GameManager.Instance?.SetFlag(K_P3Done, true);
        OnBoardChanged?.Invoke();
    }

    public void ResetForNewCase()
    {
        exGfStage = PathStage.NotStarted;
        assistantStage = PathStage.NotStarted;
        exGfP1CourtDone = false;
        assistantP1CourtDone = false;
        p3Done = false;
        OnBoardChanged?.Invoke();
    }

    void RestoreFromFlags()
    {
        if (GameManager.Instance == null) return;
        exGfStage = ReconstructExGfStage();
        assistantStage = ReconstructAssistantStage();
        exGfP1CourtDone = GameManager.Instance.GetFlag(K_ExGf_P1Court);
        assistantP1CourtDone = GameManager.Instance.GetFlag(K_Asst_P1Court);
        p3Done = false;
    }

    PathStage ReconstructExGfStage()
    {
        GameManager gm = GameManager.Instance;
        if (gm.GetFlag(K_ExGf_P2Court)) return PathStage.P2_CourtDone;
        if (gm.GetFlag(K_ExGf_P2Arr)) return PathStage.P2_Arrested;
        if (gm.GetFlag(K_ExGf_P2Pin)) return PathStage.P2_SuspectPinned;
        if (gm.GetFlag(K_ExGf_P1Arr)) return PathStage.P1_Arrested;
        if (gm.GetFlag(K_ExGf_P1Pin)) return PathStage.P1_SuspectPinned;
        return PathStage.NotStarted;
    }

    PathStage ReconstructAssistantStage()
    {
        GameManager gm = GameManager.Instance;
        if (gm.GetFlag(K_Asst_P1Arr)) return PathStage.P1_Arrested;
        if (gm.GetFlag(K_Asst_P1Pin)) return PathStage.P1_SuspectPinned;
        return PathStage.NotStarted;
    }
}
