using UnityEngine;

/// <summary>
/// Reveals the parkour challenge once the Knight collects the key (which
/// itself only appears after the dragon is defeated). Assign every
/// platform/hazard/trigger that belongs to the parkour section here - they
/// should all start INACTIVE in the scene (unticked in the Hierarchy), and
/// this script turns them on together once KeyPickup fires OnKeyCollected.
/// Attach to an empty "ParkourManager" GameObject anywhere in the scene.
/// </summary>
public class ParkourManager : MonoBehaviour
{
    public GameObject[] parkourObjects; // platforms, moving hazards, the path to the princess, etc.
    public float delayAfterKeyCollected = 1f; // small pause after picking up the key

    [Header("Camera & Background for the climb")]
    public CameraFollow cameraFollow;      // drag the Main Camera's CameraFollow component here
    public bool disableCameraBounds = true; // let the camera follow freely upward during the climb
    public SpriteRenderer backgroundRenderer; // drag your Background object's Sprite Renderer here
    public Sprite climbBackgroundSprite;      // the tall Background_ClimbCliff sprite

    void OnEnable()
    {
        KeyPickup.OnKeyCollected += HandleKeyCollected;
    }

    void OnDisable()
    {
        KeyPickup.OnKeyCollected -= HandleKeyCollected;
    }

    void HandleKeyCollected()
    {
        Invoke(nameof(RevealParkour), delayAfterKeyCollected);
    }

    void RevealParkour()
    {
        foreach (GameObject obj in parkourObjects)
        {
            if (obj != null) obj.SetActive(true);
        }

        if (disableCameraBounds && cameraFollow != null)
        {
            cameraFollow.useBounds = false; // free to scroll upward now
        }

        if (backgroundRenderer != null && climbBackgroundSprite != null)
        {
            backgroundRenderer.sprite = climbBackgroundSprite;
        }
    }
}