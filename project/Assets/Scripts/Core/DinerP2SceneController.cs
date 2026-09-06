using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Playthrough 2 — diner confrontation, two phases.
//
// Phase 1 (no lab results yet):
//   Player auto-walks to Vivian Phase 1. Her NPCInteractable sets flag
//   "p2_vivianConfrontedAboutNote" on complete and deactivates itself.
//   This controller then plays Marcus's thinking dialogue and returns control
//   to the player. The player can inspect the four diner clues and use the
//   SilentDoor to navigate to ExGF_Kitchen_P2.
//
// Phase 2 (returning from kitchen with lab_results in bag):
//   Player auto-walks to Vivian Phase 2. Her NPC dialogue plays, then the
//   chief inspector call fires, then the arrest decision panel opens.
public class DinerP2SceneController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] ClueOutcomeTable dinerP2OutcomeTable;

    [Header("NPCs")]
    [SerializeField] NPCInteractable vivianPhase1;
    [SerializeField] NPCInteractable vivianPhase2;

    [Header("Phase 1 — Auto-Walk Entry")]
    [Tooltip("Seconds to wait after scene load before auto-walking to Vivian.")]
    [SerializeField] float autoWalkEntryDelay = 0.5f;

    [Header("Phase 1 — Thinking Dialogue")]
    [Tooltip("Optional Marcus line after Vivian deflects. Plays before returning control.")]
    [SerializeField] DialogueData marcusPostConfrontDialogue;
    [SerializeField] float postConfrontDelay = 1f;

    [Header("Chief Inspector Phone Call")]
    [Tooltip("Auto-triggered when lab results arrive — plays before the arrest panel opens.")]
    [SerializeField] DialogueData chiefInspectorCallDialogue;

    [Header("Phase 2 Decision Panel")]
    [SerializeField] string phase2DecisionTitle = "You have everything. Make your move.";
    [SerializeField] float phase2OpenDelay = 1f;

    [Header("Movement")]
    [SerializeField] float characterWalkSpeed = 3f;

    const string ConfrontedNoteFlag = "p2_vivianConfrontedAboutNote";
    const string LabResultsClue     = "lab_results";

    bool _phase1PanelOpened;
    bool _phase2Started;

    void Start()
    {
        BagUI.Instance?.SetOutcomeTable(dinerP2OutcomeTable);

        foreach (Interactable i in FindObjectsOfType<Interactable>())
        {
            i.RefreshActiveState();
        }

        if (vivianPhase1 != null)
        {
            vivianPhase1.deactivateSelfOnComplete = false;
        }

        bool hasLabResults = GameManager.Instance != null && GameManager.Instance.HasClue(LabResultsClue);

        if (vivianPhase2 != null)
        {
            vivianPhase2.gameObject.SetActive(hasLabResults);
        }

        if (hasLabResults)
        {
            _phase2Started     = true;
            _phase1PanelOpened = true;
            StartCoroutine(AutoWalkToVivianPhase2());
        }
        else if (GameManager.Instance != null && GameManager.Instance.GetFlag(ConfrontedNoteFlag))
        {
            // Phase 1 confrontation already done — free roam, player uses door to kitchen.
            _phase1PanelOpened = true;
        }
        else
        {
            StartCoroutine(AutoWalkToVivianPhase1());
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClueAdded += OnClueAdded;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClueAdded -= OnClueAdded;
        }
    }

    void OnEnable()
    {
        ItemSelectionUI.PreDialogueWalkHandler = HandlePreDialogueWalk;
    }

    void OnDisable()
    {
        if (ItemSelectionUI.PreDialogueWalkHandler == HandlePreDialogueWalk)
        {
            ItemSelectionUI.PreDialogueWalkHandler = null;
        }
    }

    void Update()
    {
        if (_phase1PanelOpened || GameManager.Instance == null) return;

        if (GameManager.Instance.GetFlag(ConfrontedNoteFlag))
        {
            _phase1PanelOpened = true;
            StartCoroutine(Phase1ThinkingDialogue());
        }
    }

    void OnClueAdded(string clueID)
    {
        if (!_phase2Started && clueID == LabResultsClue)
        {
            _phase2Started     = true;
            _phase1PanelOpened = true;

            if (vivianPhase2 != null)
            {
                vivianPhase2.gameObject.SetActive(true);
            }

            StartCoroutine(AutoWalkToVivianPhase2());
        }
    }

    IEnumerator AutoWalkToVivianPhase1()
    {
        if (vivianPhase1 == null || PlayerController.Instance == null) yield break;

        PlayerController.Instance.CanMove = false;
        yield return new WaitForSeconds(autoWalkEntryDelay);

        yield return StartCoroutine(WalkToNPC(vivianPhase1.transform, () => vivianPhase1.TriggerInteract()));
    }

    IEnumerator Phase1ThinkingDialogue()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }

        yield return new WaitForSeconds(postConfrontDelay);

        if (marcusPostConfrontDialogue != null && DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(marcusPostConfrontDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        if (vivianPhase1 != null)
        {
            vivianPhase1.interactionLocked = true;
        }

        // Return control — player explores diner freely and uses the door to reach the kitchen.
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = true;
        }
    }

    IEnumerator AutoWalkToVivianPhase2()
    {
        if (vivianPhase2 == null || PlayerController.Instance == null) yield break;

        PlayerController.Instance.CanMove = false;
        yield return new WaitForSeconds(autoWalkEntryDelay);

        yield return StartCoroutine(WalkToNPC(vivianPhase2.transform, () => { }));

        if (vivianPhase2.dialogue != null && DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(vivianPhase2.dialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        StartCoroutine(OpenPhase2Panel());
    }

    IEnumerator OpenPhase2Panel()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = false;
        }

        yield return new WaitForSeconds(phase2OpenDelay);

        if (chiefInspectorCallDialogue != null && DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(chiefInspectorCallDialogue, () => done = true);
            yield return new WaitUntil(() => done);
        }

        BagUI.Instance?.ForceOpenForDecision(phase2DecisionTitle, null);
    }

    // Walk player toward the active Vivian NPC before any TriggerDialogue outcome fires.
    void HandlePreDialogueWalk(string clueID, System.Action onContinue)
    {
        NPCInteractable target = _phase2Started ? vivianPhase2 : vivianPhase1;

        if (target != null && PlayerController.Instance != null)
        {
            StartCoroutine(WalkToNPC(target.transform, onContinue));
        }
        else
        {
            onContinue?.Invoke();
        }
    }

    IEnumerator WalkToNPC(Transform npc, System.Action onArrived)
    {
        PlayerController player = PlayerController.Instance;
        player.CanMove = false;

        Vector3 playerPos = player.transform.position;
        Vector3 npcPos    = npc.position;
        Vector3 dir       = (npcPos - playerPos).normalized;
        Vector3 dest      = npcPos - dir * 1f;
        dest.z            = playerPos.z;

        if (GridPathfinder.Instance != null)
        {
            List<Vector2> path = GridPathfinder.Instance.FindPath(playerPos, dest);
            yield return StartCoroutine(CharacterMover.WalkPath(player.transform, path, characterWalkSpeed));
        }
        else
        {
            yield return StartCoroutine(CharacterMover.Walk(player.transform, dest, characterWalkSpeed));
        }

        CharacterMover.FaceEachOther(player.transform, npc);
        onArrived?.Invoke();
    }
}
