using UnityEngine;

public class DinerItemSelectionTrigger : Interactable
{
    static readonly string[] DinerClueIDs =
    {
        "diner_corner_photo",
        "diner_reservation_book",
        "diner_jukebox",
        "diner_trashbin"
    };

    protected override void OnInteract()
    {
        bool anyFound = false;
        foreach (string id in DinerClueIDs)
        {
            if (GameManager.Instance.HasClue(id))
            {
                anyFound = true;
                break;
            }
        }

        if (!anyFound)
        {
            GameOverScreen.Instance?.Show();
            return;
        }

        ItemSelectionUI.Instance?.ShowWithCallback(DinerClueIDs, OnClueChosen, "Which piece of evidence do you present?");
    }

    void OnClueChosen(string chosenClueID)
    {
        GameOverScreen.Instance?.Show();
    }
}
