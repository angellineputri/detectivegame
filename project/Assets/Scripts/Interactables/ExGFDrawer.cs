using UnityEngine;

public class ExGFDrawer : Interactable
{
    void Reset()
    {
        objectID = "exgf_drawer";
        displayName = "Locked Drawer";
        noticeText = "A bedside drawer with a small padlock on it.";
        revealText = "It won't budge. You don't have the key yet.";
    }
}
