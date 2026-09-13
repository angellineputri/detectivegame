using UnityEngine;

public class ExGFStickyNote : Interactable
{
    void Reset()
    {
        objectID = "exgf_sticky_note";
        displayName = "Sticky Note";
        noticeText = "A crumpled sticky note is tucked under the vanity mirror.";
        revealText = "'He has to pay. Meet me at the diner and bring the spare key.' The handwriting matches the victim's ex-girlfriend.";
        requiredPlaythrough = 2;
    }
}
