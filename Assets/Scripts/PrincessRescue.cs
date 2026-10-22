using UnityEngine;
using System.Collections;

/// <summary>
/// The princess at the end of the parkour section. Doesn't detect the Knight
/// itself - the surrounding CageTrigger (on the cage sprite) calls
/// BeginRescue() the moment the Knight touches the cage. Both the Knight and
/// Princess then play a simple jump/bounce celebration in place for a few
/// seconds (ending exactly where they started, so they land back on the
/// platform rather than drifting anywhere), then the win screen shows.
/// Attach to the Princess GameObject (no collider needed on her directly).
/// </summary>
public class PrincessRescue : MonoBehaviour
{
    [Header("Celebration")]
    public float celebrationDuration = 3f;
    public float jumpHeight = 0.5f;
    public float jumpSpeed = 6f;

    [Header("Win Screen")]
    public GameObject winPanel; // assign this scene's WinPanel directly - safer than relying on GameManager's cross-scene singleton

    private bool triggered = false;

    public void BeginRescue(Transform knight)
    {
        if (triggered) return;
        triggered = true;

        StartCoroutine(CelebrationSequence(knight));
    }

    IEnumerator CelebrationSequence(Transform knight)
    {
        PlayerController pc = knight.GetComponent<PlayerController>();
        Animator knightAnimator = knight.GetComponent<Animator>();

        if (knightAnimator != null)
        {
            knightAnimator.SetFloat("Speed", 0f); // snap to Idle first
            knightAnimator.enabled = false;        // then freeze entirely - stops the idle bob too
        }
        if (pc != null) pc.enabled = false; // stop input during the celebration

        Rigidbody2D knightRb = knight.GetComponent<Rigidbody2D>();
        if (knightRb != null)
        {
            knightRb.linearVelocity = Vector2.zero;
            knightRb.bodyType = RigidbodyType2D.Kinematic; // ignore gravity/physics while we hand-animate position
        }

        Vector3 knightStart = knight.position;
        Vector3 princessStart = transform.position;

        float t = 0f;
        while (t < celebrationDuration)
        {
            float bounce = Mathf.Abs(Mathf.Sin(t * jumpSpeed)) * jumpHeight;
            knight.position = knightStart + Vector3.up * bounce;
            transform.position = princessStart + Vector3.up * bounce;

            t += Time.deltaTime;
            yield return null;
        }

        // Snap back to exactly where they started - since both were standing
        // on the platform when the celebration began, this keeps them
        // resting on the platform afterward instead of floating or drifting.
        knight.position = knightStart;
        transform.position = princessStart;

        // Left Kinematic intentionally - the game is effectively over here
        // (win screen incoming), so there's no need to restore Dynamic physics.

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.WinGame(); // fallback if this scene relies on GameManager instead
        }
    }
}