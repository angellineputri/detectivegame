using UnityEngine;

public class ApartmentPhoto : Interactable
{
    void Reset()
    {
        objectID = "apartment_photo";
        displayName = "Scribbled Photo";
        noticeText = "A photo is tucked under the bed. It shows two people together, taken just a week ago.";
        revealText = "The girl's face has been scribbled out with ink. Did they recently break up? Or is something worse going on?";
    }
}
