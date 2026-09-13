using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] float moveSpeed = 4f;

    Rigidbody2D _rb;
    Animator _animator;
    Vector2 _input;
    Vector2 _autoMoveVelocity;
    float _lastMoveX = 0f;
    float _lastMoveY = -1f;

    public bool CanMove { get; set; } = true;

    public void SetAutoMoveVelocity(Vector2 v) { _autoMoveVelocity = v; }

    void Awake()
    {
        Instance = this;
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        _input = CanMove
            ? new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized
            : Vector2.zero;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            bool uiConsumed = (UIManager.Instance != null && (UIManager.Instance.IsPopupVisible || UIManager.Instance.WasJustClosed))
                           || (DialogueRunner.Instance != null && (DialogueRunner.Instance.IsPlaying || DialogueRunner.Instance.WasJustClosed));

            if (!uiConsumed && !Interactable.GlobalInteractionLocked)
            {
                TryInteractWithNearest();
            }
        }

        Vector2 animInput = _autoMoveVelocity != Vector2.zero
            ? _autoMoveVelocity.normalized
            : _input;
        float inputMagnitude = animInput.magnitude;

        if (inputMagnitude > 0f)
        {
            _lastMoveX = animInput.x;
            _lastMoveY = animInput.y;
        }

        if (_animator != null)
        {
            _animator.SetFloat("MoveX", _lastMoveX);
            _animator.SetFloat("MoveY", _lastMoveY);
            _animator.SetFloat("Speed", inputMagnitude);
        }
    }

    void TryInteractWithNearest()
    {
        Interactable nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Interactable interactable in FindObjectsOfType<Interactable>())
        {
            if (!interactable.IsActiveThisPlaythrough()) continue;
            if (interactable.interactionLocked) continue;

            Collider2D col = interactable.GetComponent<Collider2D>();
            float dist = col != null
                ? Vector2.Distance(transform.position, col.ClosestPoint(transform.position))
                : Vector2.Distance(transform.position, interactable.transform.position);

            if (dist <= interactable.interactRange && dist < nearestDist)
            {
                nearest = interactable;
                nearestDist = dist;
            }
        }

        if (nearest != null)
        {
            nearest.TriggerInteract();
        }
    }

    public void FaceDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return;
        Vector2 norm = direction.normalized;
        _lastMoveX = norm.x;
        _lastMoveY = norm.y;
        if (_animator != null)
        {
            _animator.SetFloat("MoveX", _lastMoveX);
            _animator.SetFloat("MoveY", _lastMoveY);
            _animator.SetFloat("Speed", 0f);
        }
    }

    void FixedUpdate()
    {
        if (!CanMove && _autoMoveVelocity != Vector2.zero)
            _rb.velocity = _autoMoveVelocity;
        else
            _rb.velocity = _input * moveSpeed;
    }
}
