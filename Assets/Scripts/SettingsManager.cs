using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public Resolution[] resolutions;
    public int currentResolutionIndex;

    public FullScreenMode windowMode;

    public float brightness = 1f;
    public float volume = 1f;

    public event Action OnSettingsChanged;

    public Image BrightnessImage;
    public GameObject BrightnessContainer;

    private void Awake()        // so it persists between scenes
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Initialize()
    {
        BrightnessContainer.SetActive(true);
        resolutions = Screen.resolutions;

        currentResolutionIndex = resolutions.Length - 1;
        windowMode = Screen.fullScreenMode;

        LoadSettings();
        ApplySettings();
    }

    // --------------------
    // Resolutions
    // --------------------
    public void SetResolution(int index)
    {
        currentResolutionIndex = Mathf.Clamp(index, 0, resolutions.Length - 1);
        ApplyResolution();
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void NextResolution()
    {
        SetResolution(currentResolutionIndex + 1);
    }

    public void PreviousResolution()
    {
        SetResolution(currentResolutionIndex - 1);
    }

    void ApplyResolution()
    {
        Resolution res = resolutions[currentResolutionIndex];
        Screen.SetResolution(res.width, res.height, windowMode);
    }

    // --------------------
    // Window Modes
    // --------------------
    public void NextWindowMode()
    {
        int mode = ((int)windowMode + 1) % Enum.GetValues(typeof(FullScreenMode)).Length;
        windowMode = (FullScreenMode)mode;

        ApplyResolution();
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void PreviousWindowMode()
    {
        int count = Enum.GetValues(typeof(FullScreenMode)).Length;
        int mode = ((int)windowMode - 1 + count) % count;

        windowMode = (FullScreenMode)mode;

        ApplyResolution();
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    // --------------------
    // Brightness
    // --------------------
    public void SetBrightness(float value)
    {
        brightness = Mathf.Clamp(value, 0.2f, 1f);

        Color temp = BrightnessImage.color;
        temp.a = 1f - brightness;   // brightness 1 = fully transparent overlay, 0.2 = 80% opaque
        BrightnessImage.color = temp;

        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    // --------------------
    // Volume
    // --------------------
    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);
        AudioListener.volume = volume;

        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    // --------------------
    // Save / Load
    // --------------------
    void SaveSettings()     // saving the choices to the player preferences, which are saved by unity in the windows registry, so it persists between games.
    {
        PlayerPrefs.SetInt("ResolutionIndex", currentResolutionIndex);
        PlayerPrefs.SetInt("WindowMode", (int)windowMode);
        PlayerPrefs.SetFloat("Brightness", brightness);
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        if (PlayerPrefs.HasKey("ResolutionIndex"))
            currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex");

        if (PlayerPrefs.HasKey("WindowMode"))
            windowMode = (FullScreenMode)PlayerPrefs.GetInt("WindowMode");

        brightness = PlayerPrefs.GetFloat("Brightness", 1f);
        volume = PlayerPrefs.GetFloat("Volume", 1f);
    }

    void ApplySettings()
    {
        ApplyResolution();
        SetBrightness(brightness);
        SetVolume(volume);
    }
}
