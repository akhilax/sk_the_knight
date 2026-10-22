using UnityEngine;

/// <summary>
/// A door that separates two rooms (e.g. Dwarf Room -> Dragon Room).
/// Starts locked (blocks the player with a solid collider and/or is invisible-walled).
/// Call Unlock() to open it (e.g. from HealthPotion after pickup, or DragonController on death).
/// Attach to a Door GameObject with two Collider2D: one solid "blocker" (non-trigger)
/// and one trigger "transition" zone used once unlocked.
/// </summary>
public class RoomDoor : MonoBehaviour
{
    public Collider2D blockerCollider;      // Solid wall that disappears on unlock
    public GameObject visualOpenState;      // Optional: open-door sprite (separate child object)
    public string nextRoomSceneName;        // Leave blank if rooms are just areas in one scene
    public Transform teleportDestination;   // Alternative to scene load: move player here

    private bool isLocked = true;
    private SpriteRenderer closedSpriteRenderer; // this GameObject's own sprite = the closed-door look

    void Awake()
    {
        closedSpriteRenderer = GetComponent<SpriteRenderer>();
        SetLockedVisual(true);
    }

    [ContextMenu("Test Unlock")]
    public void Unlock()
    {
        isLocked = false;
        if (blockerCollider != null) blockerCollider.enabled = false;
        SetLockedVisual(false);
    }

    void SetLockedVisual(bool locked)
    {
        // Show/hide THIS object's own sprite (the closed-door look) directly,
        // instead of toggling a GameObject reference - avoids any risk of a
        // self-referencing field accidentally deactivating this whole door.
        if (closedSpriteRenderer != null) closedSpriteRenderer.enabled = locked;
        if (visualOpenState != null) visualOpenState.SetActive(!locked);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isLocked) return;
        if (!other.CompareTag("Player")) return;

        if (!string.IsNullOrEmpty(nextRoomSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextRoomSceneName);
        }
        else if (teleportDestination != null)
        {
            other.transform.position = teleportDestination.position;
        }
    }
}