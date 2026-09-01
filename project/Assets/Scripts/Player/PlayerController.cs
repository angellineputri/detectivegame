using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] float moveSpeed = 4f;

    Rigidbody2D _rb;
    Animator _animator;
    Vector2 _input;

    public bool CanMove { get; set; } = true;

    void Awake()
    {
        Instance = this;
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!CanMove)
        {
            _input = Vector2.zero;
        }
        else
        {
            _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

            if (Input.GetKeyDown(KeyCode.Return))
            {
                bool uiConsumed = (UIManager.Instance != null && (UIManager.Instance.IsPopupVisible || UIManager.Instance.WasJustClosed))
                               || (DialogueRunner.Instance != null && (DialogueRunner.Instance.IsPlaying || DialogueRunner.Instance.WasJustClosed));

                if (!uiConsumed)
                {
                    TryInteractWithNearest();
                }
            }
        }

        if (_animator != null)
        {
            _animator.SetFloat("MoveX", _input.x);
            _animator.SetFloat("MoveY", _input.y);
        }
    }

    void TryInteractWithNearest()
    {
        Interactable nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Interactable interactable in FindObjectsOfType<Interactable>())
        {
            if (!interactable.IsActiveThisPlaythrough()) continue;

            float dist = Vector2.Distance(transform.position, interactable.transform.position);
            if (dist <= interactable.interactRange && dist < nearestDist)
            {
                nearest = interactable;
                nearestDist = dist;
            }
        }

        nearest?.TriggerInteract();
    }

    void FixedUpdate()
    {
        _rb.velocity = _input * moveSpeed;
    }
}
