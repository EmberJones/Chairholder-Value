using UnityEngine;
using UnityEngine.UI;

public class BrightnessSlider : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        slider.value = SettingsManager.Instance.brightness;
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(float value)
    {
        SettingsManager.Instance.SetBrightness(value);
    }
}
