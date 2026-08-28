using UnityEngine;

public class ChefDesk : Interactable
{
    void Reset()
    {
        objectID    = "chef_desk";
        displayName = "Recipe Notebooks";
        noticeText  = "A pile of recipe notebooks stacked on the desk.";
        revealText  = "Flipping through one, a name stops you — the victim's. A margin note reads: \"his favourite, always.\" The handwriting is careful. Almost tender.";
    }
}
