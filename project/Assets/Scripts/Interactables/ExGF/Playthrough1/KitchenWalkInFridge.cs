using UnityEngine;

public class KitchenWalkInFridge : Interactable
{
    void Reset()
    {
        objectID    = "kitchen_walkin_fridge";
        displayName = "Bloody Knife";
        noticeText  = "The walk-in fridge door is slightly open. It was not closed properly. Maybe someone left in a hurry.";
        revealText  = "On the lowest shelf, half-hidden behind some produce, is a chef's knife. The blade has dried blood on it. This must be the murder weapon.";
    }
}
