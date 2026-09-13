using UnityEngine;

public class DinerJukebox : Interactable
{
    void Reset()
    {
        objectID = "diner_jukebox";
        displayName = "Jukebox";
        noticeText = "An old jukebox sits in the corner, still warm.";
        revealText = "The ex-GF mentioned offhand that this was his favourite song. Then she went quiet and changed the subject.";
    }
}
