using UnityEngine;

public class ChefDesk : Interactable
{
    void Reset()
    {
        objectID    = "chef_desk";
        displayName = "Recipe Notebooks";
        noticeText  = "A pile of recipe notebooks stacked on the desk.";
        revealText  = "Flipping through one, you stop at a familiar name. The victim's. A note in the margin reads: \"his favourite, always.\" The handwriting is neat and careful.";
    }
}
