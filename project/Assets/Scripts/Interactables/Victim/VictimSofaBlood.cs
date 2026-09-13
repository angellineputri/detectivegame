using UnityEngine;

public class VictimSofaBlood : Interactable
{
    void Reset()
    {
        objectID    = "victim_sofa_blood";
        displayName = "Blood-Stained Sofa";
        noticeText  = "The sofa looks untouched, its cushions squared away as if no one had sat here in days.";
        revealText  = "You flip a cushion over. The underside is soaked in dried blood. Someone turned it over to hide the stain.";
    }
}
