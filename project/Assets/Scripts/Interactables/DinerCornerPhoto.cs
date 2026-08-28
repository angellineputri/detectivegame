using UnityEngine;

public class DinerCornerPhoto : Interactable
{
    void Reset()
    {
        objectID = "diner_corner_photo";
        displayName = "Torn Photo";
        noticeText = "The cushion in the corner sits slightly out of place.";
        revealText = "A torn photograph tucked underneath — a picture of the victim and a woman, ripped cleanly down the middle. Someone didn't want it found.";
    }
}
