using UnityEngine;

public class KitchenApron : Interactable
{
    void Reset()
    {
        objectID    = "kitchen_apron";
        displayName = "Stained Apron";
        noticeText  = "An apron hangs on a hook by the back wall. There's a faint reddish stain across the front.";
        revealText  = "It smells faintly of tomatoes. The stain is ketchup, not blood. Nothing suspicious here.";
    }
}
