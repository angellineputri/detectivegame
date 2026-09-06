using UnityEngine;

// P2 version of the walk-in fridge. The knife is now bagged as crime scene evidence.
// Collecting it lets the player submit it to the lab via the Kitchen_P2 outcome table
// (TriggerDialogue outcome → grants lab_results → loads ExGF_Diner_P2).
public class KitchenBloodyKnifeP2 : Interactable
{
    void Reset()
    {
        objectID            = "bloody_knife_bagged";
        displayName         = "Bagged Knife";
        noticeText          = "The walk-in fridge door is sealed with police tape — someone flagged it as a crime scene. Through the gap the chef's knife is visible, wrapped in an evidence bag.";
        revealText          = "The knife is bagged but hasn't been tested yet. Take it to the lab before Vivian realises what you have.";
        requiredPlaythrough = 2;
    }
}
