using UnityEngine;

public class ScenePortal : Interactable
{
    public string targetScene;

    protected override void Start()
    {
        addToBag = false;
        base.Start();
    }

    protected override void OnInteract()
    {
        if (string.IsNullOrEmpty(targetScene))
        {
            return;
        }

        GameManager.Instance?.LoadScene(targetScene);
    }
}
