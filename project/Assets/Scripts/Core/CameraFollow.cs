using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10f);

    public bool useBounds = true;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    Camera cam;
    bool _hasSnapped;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void OnEnable()
    {
        // Reset so the first LateUpdate after this scene's camera activates snaps immediately.
        _hasSnapped = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = DesiredPosition();

        if (!_hasSnapped)
        {
            // Snap on the first frame — Start() has already run by this point, so the player
            // is at their correct spawn position. No lerp slide from wherever the camera sat
            // in the scene file.
            transform.position = desiredPosition;
            _hasSnapped = true;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }

    Vector3 DesiredPosition()
    {
        Vector3 pos = target.position + offset;

        if (useBounds && cam != null)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth  = cam.orthographicSize * cam.aspect;

            float clampMinX = minBounds.x + halfWidth;
            float clampMaxX = maxBounds.x - halfWidth;
            float clampMinY = minBounds.y + halfHeight;
            float clampMaxY = maxBounds.y - halfHeight;

            if (clampMaxX > clampMinX)
                pos.x = Mathf.Clamp(pos.x, clampMinX, clampMaxX);
            if (clampMaxY > clampMinY)
                pos.y = Mathf.Clamp(pos.y, clampMinY, clampMaxY);
        }

        return pos;
    }
}
