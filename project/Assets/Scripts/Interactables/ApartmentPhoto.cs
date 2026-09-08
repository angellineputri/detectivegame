using UnityEngine;

public class ApartmentPhoto : Interactable
{
    void Reset()
    {
        objectID = "apartment_photo";
        displayName = "Scribbled Photo";
        noticeText = "A photo is tucked underneath the bed — a couple, dated just a week ago.";
        revealText = "The girl's face has been scribbled out. Recently broken up? Or something worse.";
    }
}
