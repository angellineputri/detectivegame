using UnityEngine;

public class ApartmentMeetingNote : Interactable
{
    void Reset()
    {
        objectID = "apartment_meeting_note";
        displayName = "Meeting Note";
        noticeText = "A planning note sits on the desk. It has a date, a time, and a meeting written on it.";
        revealText = "The handwriting does not match his usual notes. Someone else wrote this for him. Most likely his assistant.";
        interactRange = 3f;

        var sr = GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null) sr.sortingOrder = 1;
    }
}
