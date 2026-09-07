using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DinerP2SceneController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] ClueOutcomeTable dinerP2OutcomeTable;

    [Header("NPCs")]
    [SerializeField] NPCInteractable vivianPhase1;
    [SerializeField] NPCInteractable vivianPhase2Confirm;
    [SerializeField] NPCInteractable vivianPhase2;

    [Header("Entry Delay")]
    [SerializeField] float autoWalkEntryDelay = 0.5f;

    [Header("Phase 1 Thinking Dialogue")]
    [SerializeField] DialogueData marcusPostConfrontDialogue;
    [SerializeField] float postConfrontDelay = 1f;

    [Header("Kitchen Door Entry")]
    [SerializeField] Transform kitchenDoorEntryPoint;
    [SerializeField] Vector2 kitchenDoorEntryFacing = new Vector2(0f, -1f);

    [Header("Movement")]
    [SerializeField] float characterWalkSpeed = 3f;

    const string ConfrontedNoteFlag     = "p2_vivianConfrontedAboutNote";
    const string PhoneCallCompletedFlag = "p2_phoneCallCompleted";
    const string LabResultsClue         = "lab_results";

    bool _phase1PanelOpened;
    bool _confirmPhaseActive;
    bool _phase2Started;

    void Start()
    {
        BagUI.Instance?.SetOutcomeTable(dinerP2OutcomeTable);

        foreach (Interactable i in FindObjectsOfType<Interactable>())
        {
            i.RefreshActiveState();
        }

        if (kitchenDoorEntryPoint != null
            && GameManager.Instance != null
            && GameManager.Instance.LastScene == "ExGF_Kitchen_P2"
            && PlayerController.Instance != null)
        {
            PlayerController.Instance.transform.position = kitchenDoorEntryPoint.position;
            PlayerController.Instance.FaceDirection(kitchenDoorEntryFacing);
        }

        if (vivianPhase1 != null)
        {
            vivianPhase1.deactivateSelfOnComplete = false;
        }

        bool hasLabResults = GameManager.Instance != null && GameManager.Instance.HasClue(LabResultsClue);
        bool isConfronted  = GameManager.Instance != null && GameManager.Instance.GetFlag(ConfrontedNoteFlag);
        bool phoneCallDone = GameManager.Instance != null && GameManager.Instance.GetFlag(PhoneCallCompletedFlag);

        Debug.Log($"[DinerP2] Start — hasLabResults={hasLabResults}, confronted={isConfronted}, phoneCallDone={phoneCallDone} | vivianPhase2Confirm={(vivianPhase2Confirm == null ? "NULL" : vivianPhase2Confirm.name)}, vivianPhase2={(vivianPhase2 == null ? "NULL" : vivianPhase2.name)}");

        if (isConfronted && vivianPhase1 != null)
        {
            vivianPhase1.interactionLocked = true;
        }

        if (vivianPhase2Confirm != null)
        {
            vivianPhase2Confirm.gameObject.SetActive(hasLabResults && !phoneCallDone);
        }

        if (vivianPhase2 != null)
        {
            vivianPhase2.gameObject.SetActive(phoneCallDone);
        }

        if (phoneCallDone)
        {
            Debug.Log("[DinerP2] BRANCH: Phase 2B (arrest) — AutoWalkToVivianPhase2");
            _phase2Started     = true;
            _phase1PanelOpened = true;
            StartCoroutine(AutoWalkToVivianPhase2());
        }
        else if (hasLabResults)
        {
            Debug.Log($"[DinerP2] BRANCH: Phase 2A (confirm) — AutoWalkToVivianPhase2Confirm | vivianPhase2Confirm is {(vivianPhase2Confirm == null ? "NULL — NPC not in scene, coroutine will immediately yield break" : "assigned")}");
            _confirmPhaseActive = true;
            _phase1PanelOpened  = true;
            StartCoroutine(AutoWalkToVivianPhase2Confirm());
        }
        else if (isConfronted)
        {
            Debug.Log("[DinerP2] BRANCH: Phase 1 free-roam (already confronted, player explores diner / uses kitchen door)");
            _phase1PanelOpened = true;
        }
        else
        {
            Debug.Log("[DinerP2] BRANCH: Phase 1 entry — AutoWalkToVivianPhase1");
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
        if (clueID == LabResultsClue && !_phase2Started && !_confirmPhaseActive)
        {
            _confirmPhaseActive = true;
            _phase1PanelOpened  = true;

            if (vivianPhase2Confirm != null)
            {
                vivianPhase2Confirm.gameObject.SetActive(true);
            }

            StartCoroutine(AutoWalkToVivianPhase2Confirm());
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

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = true;
        }
    }

    IEnumerator AutoWalkToVivianPhase2Confirm()
    {
        if (vivianPhase2Confirm == null || PlayerController.Instance == null) yield break;

        PlayerController.Instance.CanMove = false;
        yield return new WaitForSeconds(autoWalkEntryDelay);

        yield return StartCoroutine(WalkToNPC(vivianPhase2Confirm.transform, () => { }));

        if (vivianPhase2Confirm.dialogueAfterRequirement != null && DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(vivianPhase2Confirm.dialogueAfterRequirement, () => done = true);
            yield return new WaitUntil(() => done);
        }

        GameManager.Instance?.LoadScene("ExGF_PhoneCallScene_P2");
    }

    IEnumerator AutoWalkToVivianPhase2()
    {
        if (vivianPhase2 == null || PlayerController.Instance == null) yield break;

        PlayerController.Instance.CanMove = false;
        yield return new WaitForSeconds(autoWalkEntryDelay);

        yield return StartCoroutine(WalkToNPC(vivianPhase2.transform, () => { }));

        if (vivianPhase2.dialogueAfterRequirement != null && DialogueRunner.Instance != null)
        {
            bool done = false;
            DialogueRunner.Instance.Play(vivianPhase2.dialogueAfterRequirement, () => done = true);
            yield return new WaitUntil(() => done);
        }

        GameManager.Instance?.LoadScene("ExGF_Court_P2");
    }

    void HandlePreDialogueWalk(string clueID, System.Action onContinue)
    {
        NPCInteractable target;

        if (_phase2Started)
        {
            target = vivianPhase2;
        }
        else if (_confirmPhaseActive)
        {
            target = vivianPhase2Confirm;
        }
        else
        {
            target = vivianPhase1;
        }

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
