using UnityEngine;

public class ChefBed : Interactable
{
    void Reset()
    {
        objectID    = "chef_bed";
        displayName = "Bed";
        noticeText  = "A neatly made bed. Not a wrinkle out of place.";
        revealText  = "Nothing suspicious. The chef is a tidy sleeper.";
    }
}
