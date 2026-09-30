using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System;

public class ComputerSettings : MonoBehaviour
{
    [SerializeField] private TMP_Text VolumeDisplay;
    [SerializeField] private TMP_Text BrightnessDisplay;
    private const float Step = 0.1f;   // 10% on the 0-1 scale

    private void Start()
    {
        VolumeDisplay.text = SettingsManager.Instance.volume.ToString();
        BrightnessDisplay.text = SettingsManager.Instance.brightness.ToString();
    }

    public void RaiseVolumeBy10()
    {
        SettingsManager.Instance.SetVolume(SettingsManager.Instance.volume + Step);
        UpdateVolumeDisplay();
    }

    public void LowerVolumeBy10()
    {
        SettingsManager.Instance.SetVolume(SettingsManager.Instance.volume - Step);
        UpdateVolumeDisplay();
    }

    public void RaiseBrightnessBy10()
    {
        SettingsManager.Instance.SetBrightness(SettingsManager.Instance.brightness + Step);
        UpdateBrightnessDisplay();
    }

    public void LowerBrightnessBy10()
    {
        SettingsManager.Instance.SetBrightness(SettingsManager.Instance.brightness - Step);
        UpdateBrightnessDisplay();
    }

    private void UpdateBrightnessDisplay()
    {
        BrightnessDisplay.text = Mathf.RoundToInt(SettingsManager.Instance.brightness * 100f) + "%";
    }

    private void UpdateVolumeDisplay()
    {
        VolumeDisplay.text = Mathf.RoundToInt(SettingsManager.Instance.volume * 100f) + "%";
    }

    public void ReturnToMenu()
    {
        MetaProgressManager.Instance.LoadMenu();
    }
}
