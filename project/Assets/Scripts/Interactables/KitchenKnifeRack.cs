using UnityEngine;

public class KitchenKnifeRack : Interactable
{
    void Reset()
    {
        objectID    = "kitchen_knife_rack";
        displayName = "Knife Rack";
        noticeText  = "A knife rack hangs above the head chef's station. One hook is conspicuously empty.";
        revealText  = "Seven hooks, six knives. The gap matches a chef's blade — the right size and shape for the wound. Someone removed it deliberately.";
    }
}
