using UnityEngine;

/// <summary>
/// Projectile fired by the dragon while flying. Travels in a straight line
/// toward the direction it was launched, damages the player on contact,
/// and destroys itself on impact or after a lifetime.
/// Attach to the Fireball prefab (needs Rigidbody2D + Collider2D set to Is Trigger).
/// </summary>
public class Fireball : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 15;
    public float lifetime = 5f;

    private Vector2 direction = Vector2.left;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Rotate sprite to face travel direction (optional, remove if not needed)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null) player.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}