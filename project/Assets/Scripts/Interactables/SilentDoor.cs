using UnityEngine;

// Navigation-only door: no popup, no bag entry.
// Must extend Interactable so PlayerController.TryInteractWithNearest() finds it via
// FindObjectsOfType<Interactable>(). OnInteract() is overridden to swap the sprite and
// load the target scene instead of showing a clue popup.
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

    // Always keep the collider on — door is active regardless of playthrough/flag state.
    public override void RefreshActiveState()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    // Swaps to open sprite without loading the scene.
    // Call from auto-walk sequences before walking the player to the door.
    public void OpenVisual()
    {
        if (doorRenderer != null && openSprite != null)
            doorRenderer.sprite = openSprite;
    }

    // Called by TriggerInteract() (Enter key) and auto-walk arrival.
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
