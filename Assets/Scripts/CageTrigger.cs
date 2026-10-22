using UnityEngine;

/// <summary>
/// Attach to the Cage GameObject (drawn around the Princess). Needs a
/// Collider2D set to Is Trigger, sized to roughly match the cage bars.
/// The moment the Knight touches it, the cage disappears and the princess
/// rescue celebration begins.
/// </summary>
public class CageTrigger : MonoBehaviour
{
    public PrincessRescue princess;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private bool triggered = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // Cage vanishes
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (col != null) col.enabled = false;

        if (princess != null) princess.BeginRescue(other.transform);
    }
}
