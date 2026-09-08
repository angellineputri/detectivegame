using UnityEngine;

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
