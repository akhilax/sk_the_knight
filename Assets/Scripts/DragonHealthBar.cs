using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates a UI Slider to reflect the dragon's health.
/// Attach to a Slider UI element (Canvas > Slider), set as the Dragon's health bar.
/// </summary>
public class DragonHealthBar : MonoBehaviour
{
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void OnEnable()
    {
        DragonController.OnDragonHealthChanged += UpdateBar;
        DragonController.OnDragonDefeated += HandleDragonDefeated;
    }

    void OnDisable()
    {
        DragonController.OnDragonHealthChanged -= UpdateBar;
        DragonController.OnDragonDefeated -= HandleDragonDefeated;
    }

    void UpdateBar(int current, int max)
    {
        slider.maxValue = max;
        slider.value = current;
    }

    void HandleDragonDefeated()
    {
        Invoke(nameof(HideBar), 1f);
    }

    void HideBar()
    {
        gameObject.SetActive(false);
    }
}