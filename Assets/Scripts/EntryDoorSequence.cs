using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Plays the scene-opening sequence:
///   0-1s: nothing visible
///   1-2s: closed door visible
///   2-3s: open door visible
///   3s:   Knight appears (becomes visible/controllable) at the door
///   3-4s: door stays open alongside the Knight
///   4s:   door disappears, Knight is free to play
/// Attach to an empty "EntrySequence" GameObject. Assign the closed/open
/// door child sprites and the Knight object already placed in the scene.
/// </summary>
public class EntryDoorSequence : MonoBehaviour
{
    public GameObject doorClosedVisual;
    public GameObject doorOpenVisual;
    public GameObject knight;
    public float stepDuration = 1f;

    public static event Action OnEntrySequenceComplete;

    private SpriteRenderer knightRenderer;
    private PlayerController knightController;
    private Rigidbody2D knightRb;

    void Start()
    {
        // Hide both door states and the Knight before the sequence begins.
        if (doorClosedVisual != null) doorClosedVisual.SetActive(false);
        if (doorOpenVisual != null) doorOpenVisual.SetActive(false);

        if (knight != null)
        {
            knightRenderer = knight.GetComponent<SpriteRenderer>();
            knightController = knight.GetComponent<PlayerController>();
            knightRb = knight.GetComponent<Rigidbody2D>();

            if (knightRenderer != null) knightRenderer.enabled = false;
            if (knightController != null) knightController.enabled = false; // no input/movement while hidden
            if (knightRb != null) knightRb.bodyType = RigidbodyType2D.Kinematic; // don't fall while hidden
        }

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // 1) Nothing visible
        yield return new WaitForSeconds(stepDuration);

        // 2) Closed door appears
        if (doorClosedVisual != null) doorClosedVisual.SetActive(true);
        yield return new WaitForSeconds(stepDuration);

        // 3) Swap to open door
        if (doorClosedVisual != null) doorClosedVisual.SetActive(false);
        if (doorOpenVisual != null) doorOpenVisual.SetActive(true);
        yield return new WaitForSeconds(stepDuration);

        // 4) Knight comes out
        RevealKnight();
        yield return new WaitForSeconds(stepDuration);

        // 5) Door disappears, leaving just the Knight
        if (doorOpenVisual != null) doorOpenVisual.SetActive(false);

        OnEntrySequenceComplete?.Invoke();
    }

    void RevealKnight()
    {
        if (knight == null) return;

        if (knightRenderer != null) knightRenderer.enabled = true;
        if (knightController != null) knightController.enabled = true;
        if (knightRb != null) knightRb.bodyType = RigidbodyType2D.Dynamic;
    }
}