using System;
using UnityEngine;

public enum OutcomeType
{
    ShowPopup,
    GameOver,
    LoadScene,
    TriggerDialogue,
    SetFlag,
    AdvancePlaythrough
}

[Serializable]
public class ClueOutcome
{
    [Tooltip("Must match the objectID on the corresponding Interactable.")]
    public string clueID;

    public OutcomeType outcomeType;

    [Tooltip("Scene to load for LoadScene / TriggerDialogue / AdvancePlaythrough outcomes.")]
    public string targetScene;

    [Tooltip("Dialogue to play before loading the target scene (TriggerDialogue).")]
    public DialogueData dialogue;

    [Tooltip("Optional item to add to the bag after TriggerDialogue completes. Leave objectID empty to skip.")]
    public ItemGrant itemToGrantAfterDialogue;

    [Tooltip("Flag key for SetFlag outcome.")]
    public string flagKey;

    [Tooltip("Flag value for SetFlag outcome.")]
    public bool flagValue = true;

    [Tooltip("Text shown if outcomeType is ShowPopup.")]
    [TextArea(2, 3)]
    public string popupText;
}
