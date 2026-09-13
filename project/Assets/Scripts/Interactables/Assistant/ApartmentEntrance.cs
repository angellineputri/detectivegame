using UnityEngine;

public class ApartmentEntrance : Interactable
{
    void Reset()
    {
        objectID = "apartment_entrance";
        displayName = "Apartment Entrance";
        noticeText = "The entrance to [victim]'s apartment.";
        revealText = "Upon further inspection, there are no signs of forced entry. Maybe [assistant] will have more info about this.";
    }
}
