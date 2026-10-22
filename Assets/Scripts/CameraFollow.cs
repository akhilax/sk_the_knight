using UnityEngine;

/// <summary>
/// Smoothly follows the player on the X/Y axes, keeping Z fixed for a 2D camera.
/// Optionally clamps to a bounded arena so it never scrolls past the edges of
/// the background art (leave "Use Bounds" unticked for free-scrolling levels).
/// Attach to the Main Camera.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;       // Drag the Knight here
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 1f, -10f);

    [Header("Optional Bounds (for a fixed-size arena)")]
    public bool useBounds = false;
    public float minX, maxX, minY, maxY;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}