using UnityEngine;
using TMPro;
public class ResolutionSwitcher : MonoBehaviour
{
    public TextMeshProUGUI label;

    void Start()
    {
        UpdateLabel();
        SettingsManager.Instance.OnSettingsChanged += UpdateLabel;
    }

    public void Next()
    {
        SettingsManager.Instance.NextResolution();
    }

    public void Previous()
    {
        SettingsManager.Instance.PreviousResolution();
    }

    void UpdateLabel()
    {
        var sm = SettingsManager.Instance;
        var res = sm.resolutions[sm.currentResolutionIndex];

        label.text = $"{res.width} x {res.height}";
    }
}
