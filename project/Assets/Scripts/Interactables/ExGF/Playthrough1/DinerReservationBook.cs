using UnityEngine;

public class DinerReservationBook : Interactable
{
    void Reset()
    {
        objectID = "diner_reservation_book";
        displayName = "Reservation Book";
        noticeText = "The reservation book lies open on the counter.";
        revealText = "The victim had booked a table for two that night. The booking has been crossed out, but you can still read it. He showed up. The other person did not.";
    }
}
