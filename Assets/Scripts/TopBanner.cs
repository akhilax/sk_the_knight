using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Creates a persistent black banner with the team names across the top of the screen.
// Installs itself automatically at game start; no scene setup required.
public static class TopBanner
{
    private const string BannerText = "Akhil Gokul - 25BCE5009                  Santosh Karthik - 25BCE1537";
    private const float BarHeight = 60f;
    private const float FontSize = 34f;

    private static GameObject instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        // Needed when "Enter Play Mode Options" skips domain reload.
        instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (instance != null) return;

        // Root canvas (no parent) so DontDestroyOnLoad works.
        instance = new GameObject("TopBanner");
        Object.DontDestroyOnLoad(instance);

        Canvas canvas = instance.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = instance.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Black bar stretched across the top.
        GameObject bar = new GameObject("Bar", typeof(RectTransform));
        bar.transform.SetParent(instance.transform, false);
        RectTransform barRect = bar.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(0.5f, 1f);
        barRect.anchoredPosition = Vector2.zero;
        barRect.sizeDelta = new Vector2(0f, BarHeight);

        Image barImage = bar.AddComponent<Image>();
        barImage.color = Color.black;
        barImage.raycastTarget = false;

        // White centered text filling the bar.
        GameObject label = new GameObject("Label", typeof(RectTransform));
        label.transform.SetParent(bar.transform, false);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = label.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset;
        text.text = BannerText;
        text.color = Color.white;
        text.fontSize = FontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
    }
}
