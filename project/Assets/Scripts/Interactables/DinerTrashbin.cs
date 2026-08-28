using UnityEngine;

public class DinerTrashbin : Interactable
{
    void Reset()
    {
        objectID = "diner_trashbin";
        displayName = "Burned Cloth";
        noticeText = "A trashbin sits by the kitchen door.";
        revealText = "Digging through it reveals a scrap of burned cloth — someone tried to incinerate it, but didn't finish the job.";
    }
}
