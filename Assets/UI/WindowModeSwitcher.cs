using UnityEngine;
using TMPro;
public class WindowModeSwitcher : MonoBehaviour
{
    public TextMeshProUGUI label;

    void Start()
    {
        UpdateLabel();
        SettingsManager.Instance.OnSettingsChanged += UpdateLabel;
    }

    public void Next()
    {
        SettingsManager.Instance.NextWindowMode();
    }

    public void Previous()
    {
        SettingsManager.Instance.PreviousWindowMode();
    }

    void UpdateLabel()
    {
        label.text = SettingsManager.Instance.windowMode.ToString();
    }
}
