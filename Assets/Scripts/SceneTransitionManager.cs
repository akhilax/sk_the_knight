using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Singleton that fades to black, loads the next scene, then fades back in.
/// Put this on a Canvas (Screen Space - Overlay) containing a full-screen
/// black Image, with a CanvasGroup on the same object controlling its alpha.
/// Place it in your FIRST scene (e.g. MainMenu) - it persists across every
/// subsequent scene load via DontDestroyOnLoad, so it only needs to exist once.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    public CanvasGroup fadeCanvasGroup; // full-screen black Image's CanvasGroup
    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(Fade(1f));           // fade to black
        yield return SceneManager.LoadSceneAsync(sceneName); // load the new scene while black
        yield return StartCoroutine(Fade(0f));            // fade back in
    }

    IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true; // block clicks while transitioning either direction

        float startAlpha = fadeCanvasGroup.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = targetAlpha;

        fadeCanvasGroup.blocksRaycasts = targetAlpha > 0.5f; // only block clicks while fully/mostly black
    }
}
