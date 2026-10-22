using UnityEngine;

/// <summary>
/// Waits for the entry door sequence to finish, then after a delay reveals
/// the Dragon (which should start positioned high above the screen so its
/// normal fly-to-corner movement reads as "flying in from the top") and its
/// health bar UI at the same moment.
/// Attach to an empty "DragonEntranceManager" GameObject. Both the Dragon
/// and the health bar UI object should start INACTIVE (unticked) in the scene.
/// </summary>
public class DragonEntranceManager : MonoBehaviour
{
    public GameObject dragon;
    public GameObject dragonHealthBarUI; // the Canvas/GameObject holding the dragon's health bar
    public float delayAfterDoorSequence = 3f;

    void OnEnable()
    {
        EntryDoorSequence.OnEntrySequenceComplete += HandleDoorSequenceComplete;
    }

    void OnDisable()
    {
        EntryDoorSequence.OnEntrySequenceComplete -= HandleDoorSequenceComplete;
    }

    void HandleDoorSequenceComplete()
    {
        Invoke(nameof(RevealDragon), delayAfterDoorSequence);
    }

    void RevealDragon()
    {
        if (dragon != null) dragon.SetActive(true);
        if (dragonHealthBarUI != null) dragonHealthBarUI.SetActive(true);
    }
}
