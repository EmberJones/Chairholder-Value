using UnityEngine;

public class SettingsNav : MonoBehaviour
{
    [SerializeField] GameObject GraphicsGroup;
    [SerializeField] GameObject AudioGroup;
    [SerializeField] GameObject ControllsGroup;

    void Start()
    {
        GraphicsGroup.SetActive(true);
        AudioGroup.SetActive(false);
        ControllsGroup.SetActive(false);
    }

    public void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    public void OnGraphicsButtonClicked()
    {
        GraphicsGroup.SetActive(true);
        AudioGroup.SetActive(false);
        ControllsGroup.SetActive(false);
    }

    public void OnAudioButtonClicked()
    {
        GraphicsGroup.SetActive(false);
        AudioGroup.SetActive(true);
        ControllsGroup.SetActive(false);
    }

    public void OnControllsButtonClicked()
    {
        GraphicsGroup.SetActive(false);
        AudioGroup.SetActive(false);
        ControllsGroup.SetActive(true);
    }
}
