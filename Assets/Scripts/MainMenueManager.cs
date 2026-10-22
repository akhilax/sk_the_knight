using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to an empty "MainMenuManager" GameObject in your Start Screen scene.
/// Hook the Start button's OnClick() to call StartGame().
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Exact name of your gameplay scene (must be added in File > Build Settings)")]
    public string gameplaySceneName = "SampleScene";

    public void StartGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}