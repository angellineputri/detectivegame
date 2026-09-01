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

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        if (useBounds && cam != null)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = cam.orthographicSize * cam.aspect;

            float clampMinX = minBounds.x + halfWidth;
            float clampMaxX = maxBounds.x - halfWidth;

            float clampMinY = minBounds.y + halfHeight;
            float clampMaxY = maxBounds.y - halfHeight;

            if (clampMaxX > clampMinX)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, clampMinX, clampMaxX);
            }

            if (clampMaxY > clampMinY)
            {
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, clampMinY, clampMaxY);
            }
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
