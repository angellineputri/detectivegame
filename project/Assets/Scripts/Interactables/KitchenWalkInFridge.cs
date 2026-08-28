using UnityEngine;

public class KitchenWalkInFridge : Interactable
{
    void Reset()
    {
        objectID    = "kitchen_walkin_fridge";
        displayName = "Bloody Knife";
        noticeText  = "The walk-in fridge door is slightly ajar — it wasn't closed properly, or someone left in a hurry.";
        revealText  = "On the lowest shelf, half-hidden behind a rack of produce: a chef's knife with dried blood on the blade. This is the murder weapon.";
    }
}
