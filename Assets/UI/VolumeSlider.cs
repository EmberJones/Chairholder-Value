using UnityEngine;
using UnityEngine.UI;
public class VolumeSlider : MonoBehaviour
{
    public Slider slider;
    void Start()
    {
        slider.value = SettingsManager.Instance.volume;
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(float value)
    {
        SettingsManager.Instance.SetVolume(value);
    }
}
