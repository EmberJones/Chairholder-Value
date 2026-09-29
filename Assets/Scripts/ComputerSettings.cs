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

    private void Start()
    {
        VolumeDisplay.text = SettingsManager.Instance.volume.ToString();
        BrightnessDisplay.text = SettingsManager.Instance.brightness.ToString();
    }

    public void RaiseVolumeBy10()
    {
        SettingsManager.Instance.SetVolume(SettingsManager.Instance.volume + 10);
        VolumeDisplay.text = SettingsManager.Instance.volume.ToString();
    }

    public void LowerVolumeBy10()
    {
        SettingsManager.Instance.SetVolume(SettingsManager.Instance.volume - 10);
        VolumeDisplay.text = SettingsManager.Instance.volume.ToString();
    }

    public void RaiseBrightnessBy10()
    {
        SettingsManager.Instance.SetBrightness(SettingsManager.Instance.brightness + 10);
        BrightnessDisplay.text = SettingsManager.Instance.brightness.ToString();
    }

    public void LowerBrightnessBy10()
    {
        SettingsManager.Instance.SetBrightness(SettingsManager.Instance.brightness - 10);
        BrightnessDisplay.text = SettingsManager.Instance.brightness.ToString();
    }

    public void ReturnToMenu()
    {
        MetaProgressManager.Instance.LoadMenu();
    }
}
