using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System;

public class ComputerSettings : MonoBehaviour
{
    [SerializeField] private TMP_Text VolumeDisplay;

    private void Start()
    {
        VolumeDisplay.text = SettingsManager.Instance.volume.ToString();
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

    public void ReturnToMenu()
    {
        MetaProgressManager.Instance.LoadMenu();
    }
}
