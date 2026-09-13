using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite closedSprite;
    [SerializeField] Sprite openSprite;

    public void Open()
    {
        AudioManager.Instance?.PlayDoorOpen();
        if (spriteRenderer != null && openSprite != null)
        {
            spriteRenderer.sprite = openSprite;
        }
    }

    public void Close()
    {
        if (spriteRenderer != null && closedSprite != null)
        {
            spriteRenderer.sprite = closedSprite;
        }
    }
}
