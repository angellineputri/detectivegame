using UnityEngine;

public class BedroomDrawer : Interactable
{
    void Reset()
    {
        objectID = "phone";
        displayName = "Phone";
        noticeText = "a drawer beside the bed, the victim's phone is on it.";
        revealText = "There's a reminder on the phone planning to fire [assistant].";
    }
}
