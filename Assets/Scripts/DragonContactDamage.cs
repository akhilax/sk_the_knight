using UnityEngine;

/// <summary>
/// Attach to a child GameObject of the Dragon (e.g. "DragonBodyHazard") with
/// its own Collider2D (Is Trigger = true) roughly matching the dragon's body
/// silhouette. This is separate from the Dragon's main Box Collider2D (which
/// stays on the "Enemy" layer purely for the Knight's melee attack to hit).
///
/// This hazard collider should live on its OWN layer (e.g. "DragonHazard")
/// with Player <-> DragonHazard collision ENABLED in Physics2D settings,
/// while Player <-> Enemy stays disabled as before - that's what lets the
/// Knight freely pass through/jump over enemies in general, while still
/// taking a punishing hit for touching the dragon's actual body.
/// </summary>
public class DragonContactDamage : MonoBehaviour
{
    public int damage = 15;              // same as a fireball hit
    public float knockbackForce = 10f;
    public float hitCooldown = 0.5f;      // prevents repeated damage if they stay overlapping briefly

    private float lastHitTime = -999f;

    void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // covers the case where the player gets knocked back but drifts
        // back into contact before fully clearing the hitbox
        TryDamage(other);
    }

    void TryDamage(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < lastHitTime + hitCooldown) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        lastHitTime = Time.time;

        player.TakeDamage(damage);

        Vector2 direction = (other.transform.position - transform.position);
        if (direction.sqrMagnitude < 0.01f) direction = Vector2.right; // fallback if perfectly overlapping
        player.ApplyKnockback(direction, knockbackForce);
    }
}
