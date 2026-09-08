using UnityEngine;

public class ApartmentMeetingNote : Interactable
{
    void Reset()
    {
        objectID = "apartment_meeting_note";
        displayName = "Meeting Note";
        noticeText = "A planning note sits on the desk — a date, a time, a meeting scheduled.";
        revealText = "The handwriting doesn't match his usual notebook scrawl. Someone else wrote this for him — his assistant, most likely.";
        interactRange = 3f;

        var sr = GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null) sr.sortingOrder = 1;
    }
}
