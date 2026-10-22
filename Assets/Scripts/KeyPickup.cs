using UnityEngine;
using System;

/// <summary>
/// The key the Knight collects after defeating the dragon. Stays hidden
/// until DragonController reports the dragon defeated, then appears.
/// Collecting it fires OnKeyCollected - used by ParkourManager to reveal
/// the parkour platforms only once the key is actually in hand.
/// Attach to a Key GameObject with a Collider2D (Is Trigger = true).
/// </summary>
public class KeyPickup : MonoBehaviour
{
    public static event Action OnKeyCollected;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        DragonController.OnDragonDefeated += Appear;
    }

    void OnDisable()
    {
        DragonController.OnDragonDefeated -= Appear;
    }

    void Start()
    {
        SetHidden(true); // hidden until the dragon dies
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

        SetHidden(true);
        OnKeyCollected?.Invoke();
    }
}
