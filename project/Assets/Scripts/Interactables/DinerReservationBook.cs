using UnityEngine;

public class DinerReservationBook : Interactable
{
    void Reset()
    {
        objectID = "diner_reservation_book";
        displayName = "Reservation Book";
        noticeText = "The reservation book lies open on the counter.";
        revealText = "The victim booked a table for two that night. It's been crossed out — but the booking is still there. He came. The other person didn't.";
    }
}
