using UnityEngine;

public class bedroomDrawer : Interactable
{
    void Reset()
    {
        objectID = "bedroom_drawer";
        displayName = "Bedroom Drawer";
        noticeText = "a drawer beside the bed, the victim's phone is on it.";
        revealText = "There's a reminder on the phone planning to fire [assistant].";
    }
}
