using UnityEngine;

public class DinerCornerPhoto : Interactable
{
    void Reset()
    {
        objectID = "diner_corner_photo";
        displayName = "Torn Photo";
        noticeText = "The chair in the corner sits slightly out of place.";
        revealText = "A torn photograph is tucked underneath. It shows the victim and a woman, ripped cleanly down the middle. Someone did not want this found.";
    }
}
