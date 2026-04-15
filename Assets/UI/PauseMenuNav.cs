using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuNav : MonoBehaviour
{
    [SerializeField] private GameObject SettingsCanvas;
    [SerializeField] private string MenuSceneName;
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

    public void OnReturnToMenuClicked()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public void OnSettingsClicked()
    {
        SettingsCanvas.SetActive(true);
    }

    public void OnResumeGameClicked()
    {
        // if the tick handling/resetting needs to done, do it here
    }

    public void OnHelpClicked()
    {
        // Have nothing to put here yet
    }
}
