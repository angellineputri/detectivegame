using UnityEngine;

public class BedroomDrawer : Interactable
{
    void Reset()
    {
        objectID = "phone";
        displayName = "Phone";
        noticeText = "The victim's phone is sitting on the drawer beside the bed.";
        revealText = "There's a reminder on the phone planning to fire [assistant].";
    }
}
