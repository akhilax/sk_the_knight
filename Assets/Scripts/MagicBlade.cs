using UnityEngine;
using System.Collections;

/// <summary>
/// The magic blade the Knight picks up after clearing all 3 dwarf waves.
/// Stays hidden/inactive until WaveManager reports all waves cleared, then
/// appears. Player walks into it, health resets to full, and after a short
/// delay the door to the dragon room appears (closed), waits briefly, then
/// unlocks.
/// Attach to the blade GameObject with a Collider2D (Is Trigger = true).
/// </summary>
public class MagicBlade : MonoBehaviour
{
    public RoomDoor doorToNextRoom; // Optional: door that unlocks on pickup

    [Header("Door Reveal Timing")]
    public float delayBeforeDoorAppears = 1f; // pause after picking up, before door shows up
    public float delayDoorStaysClosed = 1f;   // door visible but locked for this long before opening

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        WaveManager.OnAllWavesCleared += Appear;
    }

    void OnDisable()
    {
        WaveManager.OnAllWavesCleared -= Appear;
    }

    void Start()
    {
        // Hide visuals/collision only - do NOT deactivate the GameObject itself,
        // since that would disable this script and unsubscribe it from the event.
        SetHidden(true);
    }

    void Appear()
    {
        SetHidden(false);
    }

    void SetHidden(bool hidden)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = !hidden;
        if (col != null) col.enabled = !hidden;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.DrinkPotion();
        }

        if (doorToNextRoom != null)
        {
            StartCoroutine(DoorRevealSequence());
        }

        SetHidden(true);
    }

    IEnumerator DoorRevealSequence()
    {
        // 1) Wait after picking up, before the door appears at all.
        yield return new WaitForSeconds(delayBeforeDoorAppears);

        // 2) Door pops in, showing its closed sprite (Awake() sets locked visual automatically).
        doorToNextRoom.gameObject.SetActive(true);

        // 3) Hold on the closed look for a moment before actually unlocking it.
        yield return new WaitForSeconds(delayDoorStaysClosed);

        // 4) Now open it for real.
        doorToNextRoom.Unlock();
    }
}