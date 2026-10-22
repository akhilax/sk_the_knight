using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple singleton that tracks overall game state (playing / won / lost)
/// and handles the win/lose screens. Attach to an empty "GameManager"
/// GameObject that exists in your first scene (mark DontDestroyOnLoad
/// if your rooms are separate scenes).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels (assign in Inspector)")]
    public GameObject winPanel;
    public GameObject losePanel;

    private bool gameOver = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        PlayerController.OnPlayerDied += LoseGame;
    }

    void OnDisable()
    {
        PlayerController.OnPlayerDied -= LoseGame;
    }

    public void WinGame()
    {
        if (gameOver) return;
        gameOver = true;

        Time.timeScale = 0f;
        if (winPanel != null) winPanel.SetActive(true);
    }

    public void LoseGame()
    {
        if (gameOver) return;
        gameOver = true;

        Time.timeScale = 0f;
        if (losePanel != null) losePanel.SetActive(true);
    }

    // Hook this to a "Restart" button's OnClick()
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        gameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}