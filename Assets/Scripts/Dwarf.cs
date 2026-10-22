using UnityEngine;
using System;

/// <summary>
/// Basic dwarf enemy: walks toward the player when in range, attacks in melee range,
/// takes damage, and reports its death to the WaveManager.
/// Attach to each Dwarf prefab.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Dwarf : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 40;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectRange = 6f;
    public float attackRange = 1f;

    [Header("Combat")]
    public int attackDamage = 10;
    public float attackCooldown = 1.2f;
    private float lastAttackTime = -999f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isDead = false;
    private Vector3 startScale;

    // Fired when this dwarf dies, so WaveManager can decrement its counter.
    public static event Action<Dwarf> OnDwarfDied;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        startScale = transform.localScale; // remember prefab's intended size
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Flip to face player, keeping the prefab's original scale magnitude
        float facing = player.position.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector3(Mathf.Abs(startScale.x) * facing, startScale.y, startScale.z);

        if (distance <= attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (animator) animator.SetFloat("Speed", 0f);
            TryAttack();
        }
        else
        {
            float dir = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
            if (animator) animator.SetFloat("Speed", 1f);
        }
    }

    void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        lastAttackTime = Time.time;

        if (animator) animator.SetTrigger("Attack");

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.TakeDamage(attackDamage);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (animator) animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        if (animator) animator.SetTrigger("Die");

        OnDwarfDied?.Invoke(this);

        // Destroy after a short delay so the death animation can play.
        Destroy(gameObject, 1.2f);
    }
}