using UnityEngine;

public class DinerTrashbin : Interactable
{
    void Reset()
    {
        objectID = "diner_trashbin";
        displayName = "Burned Cloth";
        noticeText = "A trashbin sits by the kitchen door.";
        revealText = "Digging through it, you find a scrap of burned cloth. Someone tried to burn it completely but did not finish.";
    }
}
