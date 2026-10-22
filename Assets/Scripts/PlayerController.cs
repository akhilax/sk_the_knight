using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Controls the Knight: movement, jumping, melee attacking, health, and potion use.
/// Attach to the Knight GameObject.
/// Requires: Rigidbody2D, Collider2D, Animator (optional but recommended).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.15f;

    [Header("Combat")]
    public Transform attackPoint;       // Empty child GameObject placed in front of the knight
    public float attackRange = 0.6f;
    public int attackDamage = 20;
    public float attackCooldown = 0.4f;
    public LayerMask enemyLayer;        // Set to a layer containing Dwarves/Dragon

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    // Events other systems (UI, GameManager) can subscribe to
    public static event Action<int, int> OnHealthChanged; // (current, max)
    public static event Action OnPlayerDied;

    private Rigidbody2D rb;
    private Animator animator;
    private float horizontalInput;
    private bool isGrounded;
    private float lastAttackTime = -999f;
    private bool isDead = false;
    private Vector3 startScale;
    private float knockbackTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        startScale = transform.localScale;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        if (isDead) return;

        HandleInput();
        HandleAnimator();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime; // let the knockback velocity play out undisturbed
        }
        else
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Flip sprite to face movement direction, preserving current scale magnitude
        if (horizontalInput > 0) transform.localScale = new Vector3(Mathf.Abs(startScale.x), startScale.y, startScale.z);
        else if (horizontalInput < 0) transform.localScale = new Vector3(-Mathf.Abs(startScale.x), startScale.y, startScale.z);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (animator) animator.SetTrigger("Jump");
        }

        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0)) // attack input
        {
            TryAttack();
        }
    }

    void HandleAnimator()
    {
        if (!animator) return;
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetBool("IsGrounded", isGrounded);
    }

    void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        lastAttackTime = Time.time;

        if (animator) animator.SetTrigger("Attack");

        // Actually deal damage - call this from an Animation Event for best timing,
        // or call directly here for a simpler setup.
        DealDamage();
    }

    // Hook this to an Animation Event on the attack swing frame for precise timing.
    public void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die(); // fatal hit - go straight to Die, don't also fire Hurt this same frame
        }
        else
        {
            if (animator) animator.SetTrigger("Hurt");
        }
    }

    public void DrinkPotion()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (animator) animator.SetTrigger("Drink");
    }

    /// <summary>
    /// Shoves the player away, briefly overriding normal movement input.
    /// Call alongside TakeDamage() for hazards that should also knock back
    /// (e.g. touching the dragon's body).
    /// </summary>
    public void ApplyKnockback(Vector2 direction, float force, float lockDuration = 0.25f)
    {
        if (isDead) return;

        rb.linearVelocity = direction.normalized * force;
        knockbackTimer = lockDuration;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        if (animator)
        {
            animator.ResetTrigger("Hurt"); // clear any pending Hurt trigger so it can't compete with Die
            animator.SetTrigger("Die");
            StartCoroutine(LogAnimatorStateForDebug());
        }
        OnPlayerDied?.Invoke();
    }

    IEnumerator LogAnimatorStateForDebug()
    {
        yield return new WaitForSecondsRealtime(0.3f); // give the transition time to actually happen
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log("[PlayerController] 0.3s after Die(), Animator is in state: " + info.fullPathHash +
                   " | IsName('Knight_Die')=" + info.IsName("Knight_Die") +
                   " | IsName('Knight_Idle')=" + info.IsName("Knight_Idle") +
                   " | IsName('Knight_Hurt')=" + info.IsName("Knight_Hurt"));
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}