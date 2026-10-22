using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Shows a Lose Panel when the Knight dies, and provides a Restart button
/// that reloads the current scene. Scene-local by design (doesn't rely on
/// GameManager's cross-scene singleton), same reasoning as PrincessRescue's
/// win panel handling - a plain scene object reference just works reliably.
/// Attach to an empty "LosePanelController" GameObject in each scene where
/// the Knight can die.
/// </summary>
public class LosePanelController : MonoBehaviour
{
    public GameObject losePanel;
    public float delayBeforePanel = 1f; // lets the Die animation actually play before time freezes

    [Tooltip("Leave blank to reload the current scene. Fill in a scene name (e.g. 'MainMenue') to go there instead.")]
    public string targetSceneOnRestart = "";

    void OnEnable()
    {
        PlayerController.OnPlayerDied += HandlePlayerDied;
        Debug.Log("[LosePanelController] Subscribed to OnPlayerDied.");
    }

    void OnDisable()
    {
        PlayerController.OnPlayerDied -= HandlePlayerDied;
    }

    void HandlePlayerDied()
    {
        Debug.Log("[LosePanelController] HandlePlayerDied() was called - starting coroutine.");
        StartCoroutine(ShowLosePanelAfterDelay());
    }

    [ContextMenu("Test Show Lose Panel")]
    public void TestShowLosePanel()
    {
        Debug.Log("[LosePanelController] Manual test triggered.");
        StartCoroutine(ShowLosePanelAfterDelay());
    }

    IEnumerator ShowLosePanelAfterDelay()
    {
        Debug.Log("[LosePanelController] Coroutine started, waiting " + delayBeforePanel + "s (real-time)...");
        yield return new WaitForSecondsRealtime(delayBeforePanel);

        Debug.Log("[LosePanelController] Wait finished. losePanel is " + (losePanel == null ? "NULL" : "assigned: " + losePanel.name));

        if (losePanel != null)
        {
            losePanel.SetActive(true);
            Time.timeScale = 0f;
            Debug.Log("[LosePanelController] Panel activated and time paused.");
        }
    }

    // Hook this to the Restart button's OnClick()
    public void RestartLevel()
    {
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(targetSceneOnRestart))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            SceneManager.LoadScene(targetSceneOnRestart);
        }
    }
}