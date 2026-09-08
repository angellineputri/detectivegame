using UnityEngine;

public class SilentDoor : Interactable
{
    [Header("Door")]
    [SerializeField] SpriteRenderer doorRenderer;
    [SerializeField] Sprite openSprite;
    [SerializeField] string targetScene;

    protected override void Start()
    {
        addToBag            = false;
        requiredPlaythrough = 0;
        requiredFlag        = string.Empty;
        base.Start();

        if (doorRenderer == null)
            doorRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    public override void RefreshActiveState()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    public void OpenVisual()
    {
        if (doorRenderer != null && openSprite != null)
            doorRenderer.sprite = openSprite;
    }

    public void Interact()
    {
        OpenVisual();
        GameManager.Instance?.LoadScene(targetScene);
    }

    protected override void OnInteract()
    {
        Interact();
    }
}
