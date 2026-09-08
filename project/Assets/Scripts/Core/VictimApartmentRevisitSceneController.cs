using System.Collections;
using UnityEngine;

public class VictimApartmentRevisitSceneController : MonoBehaviour
{
    [Header("Scene Interactables")]
    [SerializeField] Interactable noteInteractable;
    [SerializeField] Interactable photoInteractable;

    [Header("Outcome Table")]
    [Tooltip("Entry for apartment_meeting_note: TriggerDialogue → chief reveal dialogue → targetScene = first assistant scene.")]
    [SerializeField] ClueOutcomeTable outcomeTable;

    [Header("Opening Description")]
    [Tooltip("Popup shown when the player arrives. Leave empty to skip.")]
    [SerializeField, TextArea(2, 4)] string openingDescription;
    [Tooltip("Optional second page of the opening popup.")]
    [SerializeField, TextArea(2, 4)] string openingRevealText;

    [Header("Decision Panel")]
    [SerializeField] string decisionTitle = "What stands out?";

    void Awake()
    {
        BagUI.Instance?.ClearBag();
    }

    void Start()
    {
        BagUI.Instance?.SetOutcomeTable(outcomeTable);
        BagUI.Instance?.SetHudVisible(true);

        if (photoInteractable != null)
            Destroy(photoInteractable);

        foreach (Interactable i in FindObjectsOfType<Interactable>())
            i.RefreshActiveState();

        if (PlayerController.Instance != null)
            PlayerController.Instance.CanMove = false;

        StartCoroutine(OpeningSequence());
    }

    void OnEnable()
    {
        ItemSelectionUI.SceneLoadInterceptHandler = HandleSceneLoadIntercept;
    }

    void OnDisable()
    {
        if (ItemSelectionUI.SceneLoadInterceptHandler == HandleSceneLoadIntercept)
            ItemSelectionUI.SceneLoadInterceptHandler = null;
    }

    void HandleSceneLoadIntercept(string clueID, System.Action proceedWithLoad)
    {
        if (PlayerController.Instance != null)
            GameManager.Instance?.SetPendingSpawn(PlayerController.Instance.transform.position);
        proceedWithLoad?.Invoke();
    }

    IEnumerator OpeningSequence()
    {
        yield return new WaitForSeconds(0.5f);

        if (!string.IsNullOrEmpty(openingDescription) && UIManager.Instance != null)
        {
            bool done = false;
            UIManager.Instance.ShowCluePopup(openingDescription, openingRevealText, () => done = true);
            yield return new WaitUntil(() => done);
        }
        else if (PlayerController.Instance != null)
        {
            PlayerController.Instance.CanMove = true;
        }
    }
}
