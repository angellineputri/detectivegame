using UnityEngine;

// P2 knife-rack evidence. The bloody knife has been recovered by police and sealed
// in an evidence bag on its hook — but never sent to the lab. Collecting it lets
// the player submit it via the Kitchen_P2 outcome table (TriggerDialogue →
// grants lab_results → loads ExGF_Diner_P2).
public class KitchenBloodyKnifeP2 : Interactable
{
    void Reset()
    {
        objectID            = "bloody_knife_bagged";
        displayName         = "Bagged Knife";
        noticeText          = "The chef's knife is back on its hook, sealed in a police evidence bag. It's been tagged but never sent to the lab.";
        revealText          = "The bag is sitting right there, untested. If those fingerprints match who you think they do, this is everything you need.";
        requiredPlaythrough = 2;
    }
}
