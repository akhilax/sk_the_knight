using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Dragon boss that alternates between two top corners of the arena:
///   FLYING  - hovers at a corner, fires a fireball roughly once per second
///             for ~7 seconds total. Cannot be damaged during this phase.
///   LANDED  - drops to the ground for 3 seconds, vulnerable to melee.
/// After landing, it picks the OTHER corner and repeats. Cycle continues
/// until health reaches 0, then it falls/dies and fires OnDragonDefeated
/// (used to trigger the death animation and unlock the parkour section).
/// Attach to the Dragon GameObject.
/// </summary>
public class DragonController : MonoBehaviour, IDamageable
{
    public enum State { Intro, Flying, Landing, Landed, TakingOff, Dying, Dead }

    [Header("Stats")]
    public int maxHealth = 300;
    private int currentHealth;

    [Header("Corners (top-left / top-right hover points)")]
    public Transform leftCorner;
    public Transform rightCorner;
    public Transform groundPositionLeft;   // where it lands under the left corner
    public Transform groundPositionRight;  // where it lands under the right corner

    [Header("Flying Phase")]
    public float flyDuration = 7f;          // total time airborne per cycle
    public float fireballInterval = 1f;     // one fireball roughly every second
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;

    [Header("Landed Phase")]
    public float landedDuration = 3f;       // vulnerable time on the ground

    [Header("Death")]
    public float fallSpeed = 4f;
    public float fallDuration = 1.5f;

    [Header("Refs")]
    public Transform player;

    public static event Action OnDragonDefeated;
    public static event Action<State> OnDragonStateChanged;
    public static event Action<int, int> OnDragonHealthChanged; // (current, max)

    private Animator animator;
    private State currentState;
    private bool onLeftSide = true; // which corner it's currently using
    private Coroutine stateRoutine;
    private Vector3 startScale;

    void Awake()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        startScale = transform.localScale;
    }

    void Update()
    {
        FacePlayer();
    }

    void FacePlayer()
    {
        if (player == null) return;
        if (currentState == State.Dying || currentState == State.Dead) return; // don't fight the death tumble rotation

        float facing = player.position.x >= transform.position.x ? 1f : -1f;
        transform.localScale = new Vector3(Mathf.Abs(startScale.x) * facing, startScale.y, startScale.z);
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        ChangeState(State.Intro);
        OnDragonHealthChanged?.Invoke(currentHealth, maxHealth);
        stateRoutine = StartCoroutine(BossLoop());
    }

    IEnumerator BossLoop()
    {
        yield return new WaitForSeconds(1.5f); // intro delay (roar animation, etc.)

        while (currentHealth > 0)
        {
            yield return StartCoroutine(FlyingPhase());
            if (currentHealth <= 0) break;

            yield return StartCoroutine(LandingPhase());
            if (currentHealth <= 0) break;

            yield return StartCoroutine(LandedPhase());
            if (currentHealth <= 0) break;

            yield return StartCoroutine(TakeOffPhase());

            onLeftSide = !onLeftSide; // switch to the other corner for the next cycle
        }
    }

    Transform CurrentCorner() => onLeftSide ? leftCorner : rightCorner;
    Transform CurrentGroundPos() => onLeftSide ? groundPositionLeft : groundPositionRight;

    // --- FLYING: hover at current corner, invulnerable, fires fireballs ---
    IEnumerator FlyingPhase()
    {
        ChangeState(State.Flying);

        // Move to the corner first (fast snap-ish glide)
        Transform corner = CurrentCorner();
        while (corner != null && Vector3.Distance(transform.position, corner.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, corner.position, 8f * Time.deltaTime);
            yield return null;
        }

        float elapsed = 0f;
        float fireTimer = 0f;

        while (elapsed < flyDuration && currentHealth > 0)
        {
            // gentle hover bob while stationed at the corner
            float bob = Mathf.Sin(Time.time * 2f) * 0.15f;
            if (corner != null)
                transform.position = new Vector3(corner.position.x, corner.position.y + bob, corner.position.z);

            fireTimer += Time.deltaTime;
            if (fireTimer >= fireballInterval)
            {
                fireTimer = 0f;
                ShootFireball();
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void ShootFireball()
    {
        if (fireballPrefab == null || fireballSpawnPoint == null || player == null) return;

        if (animator) animator.SetTrigger("Breathe");

        GameObject fb = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);
        Fireball fireballScript = fb.GetComponent<Fireball>();
        if (fireballScript != null)
        {
            Vector2 direction = (player.position - fireballSpawnPoint.position).normalized;
            fireballScript.SetDirection(direction);
        }
    }

    // --- LANDING: brief transition, still invulnerable ---
    IEnumerator LandingPhase()
    {
        ChangeState(State.Landing);
        if (animator) animator.SetTrigger("Land");

        float t = 0f;
        float duration = 1f;
        Vector3 start = transform.position;
        Transform groundPos = CurrentGroundPos();
        Vector3 end = groundPos != null ? groundPos.position : transform.position;

        while (t < duration)
        {
            transform.position = Vector3.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
    }

    // --- LANDED: vulnerable to melee for a fixed duration ---
    IEnumerator LandedPhase()
    {
        ChangeState(State.Landed);
        yield return new WaitForSeconds(landedDuration);
    }

    // --- TAKE OFF: brief transition back to flying, invulnerable ---
    IEnumerator TakeOffPhase()
    {
        ChangeState(State.TakingOff);
        if (animator) animator.SetTrigger("TakeOff");
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// Damage now applies regardless of flying/landed state - the only real
    /// limiter is whether the Knight's melee attack range can physically
    /// reach the dragon (which naturally means it's mostly only landed
    /// attacks that connect, without hardcoding an invulnerability flag).
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        OnDragonHealthChanged?.Invoke(currentHealth, maxHealth);
        if (animator) animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (stateRoutine != null) StopCoroutine(stateRoutine);
            StartCoroutine(DeathSequence());
        }
    }

    IEnumerator DeathSequence()
    {
        ChangeState(State.Dying);
        if (animator) animator.SetTrigger("Die");

        GetComponent<Collider2D>().enabled = false;

        // Simple fall-down death animation: drop off the bottom of the screen
        float t = 0f;
        while (t < fallDuration)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            transform.Rotate(0, 0, 90f * Time.deltaTime); // tumble as it falls
            t += Time.deltaTime;
            yield return null;
        }

        ChangeState(State.Dead);
        OnDragonDefeated?.Invoke();
        gameObject.SetActive(false); // hide it once the fall finishes
    }

    void ChangeState(State newState)
    {
        currentState = newState;
        OnDragonStateChanged?.Invoke(newState);

        if (animator)
        {
            animator.SetBool("IsLanded", newState == State.Landed);
            animator.SetBool("IsFlying", newState == State.Flying);
        }
    }
}