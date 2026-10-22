using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates a UI Slider to reflect the player's health.
/// Attach to a Slider UI element (Canvas > Slider), set as Health Bar.
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void OnEnable()
    {
        PlayerController.OnHealthChanged += UpdateBar;
    }

    void OnDisable()
    {
        PlayerController.OnHealthChanged -= UpdateBar;
    }

    void UpdateBar(int current, int max)
    {
        slider.maxValue = max;
        slider.value = current;
    }
}
