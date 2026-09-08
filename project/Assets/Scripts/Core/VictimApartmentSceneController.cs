using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VictimApartmentSceneController : MonoBehaviour
{
    public static VictimApartmentSceneController Instance { get; private set; }

    [Header("Outcome Table")]
    [SerializeField] ClueOutcomeTable outcomeTable;

    [Header("Scene Interactables")]
    [Tooltip("The 'Notes' GameObject with ApartmentMeetingNote attached.")]
    [SerializeField] Interactable noteInteractable;
    [Tooltip("The 'Photo' GameObject with ApartmentPhoto attached.")]
    [SerializeField] Interactable photoInteractable;

    [Header("Chief Dialogue — Onboarding")]
    [SerializeField] DialogueData chiefOpening;
    [SerializeField] DialogueData chiefFirstItem;
    [SerializeField] DialogueData chiefBagLine1;
    [SerializeField] DialogueData chiefBagLine2;
    [SerializeField] DialogueData chiefFindSecond;
    [SerializeField] DialogueData chiefBothFound;

    [Header("Chief Dialogue — Branch Reveal")]
    [SerializeField] DialogueData chiefPickedAssistant;
    [SerializeField] DialogueData chiefPickedExGF;

    [Header("Assistant Branch")]
    [Tooltip("Scene to load when the player picks the meeting note. Position is saved so the apartment can restore it.")]
    [SerializeField] string assistantSceneName = "apartment";

    [Header("Decision Panel")]
    [SerializeField] string decisionTitle = "Which lead do you follow?";

    int _clueCount;
    bool _proximityTriggered;

    Image _overlayImage;

    void Awake() => Instance = this;

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Interactable.GlobalHighlightOverride = false;
        Interactable.GlobalInteractionLocked = false;
        if (_overlayImage != null) Destroy(_overlayImage.gameObject);
        if (GameManager.Instance != null)
            GameManager.Instance.OnClueAdded -= HandleClueAdded;
        ItemSelectionUI.SceneLoadInterceptHandler = null;
    }

    void Start()
    {
        if (BagUI.Instance != null)
        {
            BagUI.Instance.SetOutcomeTable(outcomeTable);
            BagUI.Instance.SetHudVisible(false);
        }

        if (GameManager.Instance != null)
            GameManager.Instance.OnClueAdded += HandleClueAdded;

        if (PlayerController.Instance != null)
            PlayerController.Instance.CanMove = false;

        Interactable.GlobalInteractionLocked = true;

        StartCoroutine(TutorialSequence());
    }

    void HandleClueAdded(string _) => _clueCount++;

    IEnumerator TutorialSequence()
    {
        yield return new WaitForSeconds(0.5f);

        yield return PlayDialogue(chiefOpening);

        if (PlayerController.Instance != null) PlayerController.Instance.CanMove = true;

        yield return new WaitUntil(AnyClueInRange);

        if (PlayerController.Instance != null) PlayerController.Instance.CanMove = false;
        Interactable.GlobalHighlightOverride = true;

        yield return new WaitForSeconds(1f);

        yield return PlayDialogue(chiefFirstItem);

        Interactable.GlobalInteractionLocked = false;

        int clueSnapshot = _clueCount;
        yield return new WaitUntil(() => _clueCount > clueSnapshot);

        Interactable.GlobalHighlightOverride = false;

        if (PlayerController.Instance != null) PlayerController.Instance.CanMove = false;

        SetOverlay(true);
        BagUI.Instance?.SetHudPinned(true);
        DialogueRunner.Instance?.SetDimOverlayEnabled(false);
        DialogueRunner.Instance?.SetKeyboardAdvanceDisabled(true);
        yield return PlayDialogue(chiefBagLine1);
        BagUI.Instance?.SetHudPinned(false);
        SetOverlay(false);

        Interactable.GlobalInteractionLocked = true;
        yield return new WaitUntil(() => BagUI.Instance != null && BagUI.Instance.IsReviewPanelOpen);
        Interactable.GlobalInteractionLocked = false;

        SetOverlay(true);
        BagUI.Instance?.SetHudPinned(true);
        BagUI.Instance?.EnterTutorialCloseOnly();
        DialogueRunner.Instance?.SetDimOverlayEnabled(false);
        DialogueRunner.Instance?.SetKeyboardAdvanceDisabled(true);
        yield return PlayDialogue(chiefBagLine2);
        BagUI.Instance?.SetHudPinned(false);
        SetOverlay(false);
        BagUI.Instance?.ExitTutorialCloseOnly();

        yield return PlayDialogue(chiefFindSecond);
        if (PlayerController.Instance != null) PlayerController.Instance.CanMove = true;

        int afterFirst = _clueCount;
        yield return new WaitUntil(() => _clueCount > afterFirst);

        if (PlayerController.Instance != null) PlayerController.Instance.CanMove = false;
        yield return PlayDialogue(chiefBothFound);
        if (PlayerController.Instance != null) PlayerController.Instance.CanMove = true;

        ItemSelectionUI.SceneLoadInterceptHandler = (clueID, loadScene) =>
            StartCoroutine(BranchDialogueThenLoad(clueID, loadScene));
    }

    bool AnyClueInRange()
    {
        if (_proximityTriggered) return true;
        if (ClueInRange(noteInteractable) || ClueInRange(photoInteractable))
        {
            _proximityTriggered = true;
            return true;
        }
        return false;
    }

    bool ClueInRange(Interactable i)
    {
        if (i == null || PlayerController.Instance == null) return false;
        return Vector2.Distance(
            PlayerController.Instance.transform.position,
            i.transform.position) <= i.interactRange;
    }

    IEnumerator BranchDialogueThenLoad(string clueID, System.Action loadScene)
    {
        if (PlayerController.Instance != null) PlayerController.Instance.CanMove = false;
        DialogueData branch = clueID == "apartment_meeting_note"
            ? chiefPickedAssistant
            : chiefPickedExGF;
        yield return PlayDialogue(branch);

        if (clueID == "apartment_meeting_note")
        {
            if (PlayerController.Instance != null)
                GameManager.Instance?.SetPendingSpawn(PlayerController.Instance.transform.position);
            GameManager.Instance?.LoadScene(assistantSceneName);
        }
        else
        {
            loadScene?.Invoke();
        }
    }

    IEnumerator PlayDialogue(DialogueData data)
    {
        if (data == null || DialogueRunner.Instance == null) yield break;
        bool done = false;
        DialogueRunner.Instance.Play(data, () => done = true);
        yield return new WaitUntil(() => done);
    }

    void EnsureOverlay()
    {
        if (_overlayImage != null) return;

        Canvas canvas = BagUI.Instance != null
            ? BagUI.Instance.GetComponentInChildren<Canvas>()
            : null;

        if (canvas == null)
        {
            Debug.LogWarning("[VictimApartment] Could not find PersistentSystems canvas for overlay.");
            return;
        }

        var go = new GameObject("[VAOverlay]", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        go.transform.SetAsFirstSibling();

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        _overlayImage = go.AddComponent<Image>();
        _overlayImage.color = new Color(0f, 0f, 0f, 0.85f);
        _overlayImage.raycastTarget = true;
        go.SetActive(false);
    }

    void SetOverlay(bool active)
    {
        EnsureOverlay();
        if (_overlayImage != null)
            _overlayImage.gameObject.SetActive(active);
    }
}
