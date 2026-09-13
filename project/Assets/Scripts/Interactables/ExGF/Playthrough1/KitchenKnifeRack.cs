using UnityEngine;

public class KitchenKnifeRack : Interactable
{
    void Reset()
    {
        objectID    = "kitchen_knife_rack";
        displayName = "Knife Rack";
        noticeText  = "A knife rack hangs above the head chef's station. One hook is noticeably empty.";
        revealText  = "Seven hooks, six knives. The empty spot is the right size for a chef's blade, matching the wound found on the victim. Someone removed it on purpose.";
    }
}
