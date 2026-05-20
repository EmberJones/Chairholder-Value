using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class MainMenuNav : MonoBehaviour
{
    [SerializeField] private GameObject SettingsCanvas;
    [SerializeField] private string GameSceneName;
    [SerializeField] private GameObject HelpScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SettingsCanvas.SetActive(false);
    }

    public void OnExitClicked()
    {
        #if UNITY_EDITOR
                // This stops play mode in the Unity Editor
                UnityEditor.EditorApplication.isPlaying = false;
        #endif

        Application.Quit();
        
    }

    public void OnPlayGameClicked()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    public void OnSettingsClicked()
    {
        SettingsCanvas.SetActive(true);
    }

    public void OnHelpClicked()
    {
        HelpScreen.SetActive(true);
    }

    public void CloseHelpClicked()
    {
        HelpScreen.SetActive(false);
    }
}
