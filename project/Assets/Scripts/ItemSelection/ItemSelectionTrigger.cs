using UnityEngine;

public class ItemSelectionTrigger : Interactable
{
    [Header("Item Selection")]
    public ClueOutcomeTable outcomeTable;

    [Tooltip("All clue IDs that are valid candidates to present at this point.")]
    public string[] presentableClueIDs;

    [TextArea(1, 2)]
    public string selectionPrompt = "Which piece of evidence do you present?";

    protected override void OnInteract()
    {
        ItemSelectionUI.Instance?.Show(presentableClueIDs, outcomeTable, selectionPrompt);
    }
}
