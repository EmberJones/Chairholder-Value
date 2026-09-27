using System.Collections;
using UnityEngine;


public class ComputerDesktop : MonoBehaviour
{
    [Header("Apps")]
    [SerializeField] private GameObject desktop;
    [SerializeField] private GameObject mailApp;
    [SerializeField] private GameObject browserApp;
    [SerializeField] private GameObject settingsApp;

    [Header("Power")]
    [Tooltip("Parent of Desktop + all apps. It collapses like an old CRT when switched off.")]
    [SerializeField] private RectTransform screenContent;
    [SerializeField] private CanvasGroup screenContentGroup;
    [SerializeField] private float collapseTime = 0.18f;
    [SerializeField] private bool startPoweredOn = true;

    private bool isOn;
    private bool isAnimating;

    private static readonly Vector3 FullSize = Vector3.one;
    private static readonly Vector3 LineSize = new Vector3(1f, 0.01f, 1f);
    private static readonly Vector3 DotSize  = new Vector3(0f, 0.01f, 1f);

    private void Start()
    {
        isOn = startPoweredOn;
        screenContent.localScale = isOn ? FullSize : DotSize;
        SetInteractable(isOn);
        ShowDesktop();
    }

    
    public void OpenMail()    => OpenApp(mailApp);
    public void OpenBrowser() => OpenApp(browserApp);
    public void OpenSettings() => OpenApp(settingsApp);

    
    public void ShowDesktop()
    {
        SetActiveSafe(mailApp, false);
        SetActiveSafe(browserApp, false);
        SetActiveSafe(settingsApp, false);
        SetActiveSafe(desktop, true);
    }

    private void OpenApp(GameObject app)
    {
        if (!isOn || app == null) return;
        ShowDesktop();         
        app.SetActive(true);   
    }

    
    public void TogglePower()
    {
        if (isAnimating) return;
        StartCoroutine(isOn ? PowerOff() : PowerOn());
    }

    private IEnumerator PowerOff()
    {
        isAnimating = true;
        isOn = false;
        SetInteractable(false);
        yield return ScaleTo(LineSize, collapseTime);        
        yield return ScaleTo(DotSize, collapseTime * 0.6f);  
        isAnimating = false;
    }

    private IEnumerator PowerOn()
    {
        isAnimating = true;
        ShowDesktop();                                       
        yield return ScaleTo(LineSize, collapseTime * 0.6f);
        yield return ScaleTo(FullSize, collapseTime);
        isOn = true;
        SetInteractable(true);
        isAnimating = false;
    }

    private IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = screenContent.localScale;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;   
            screenContent.localScale = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }
        screenContent.localScale = target;
    }

    private void SetInteractable(bool value)
    {
        if (screenContentGroup == null) return;
        screenContentGroup.interactable = value;
        screenContentGroup.blocksRaycasts = value;
    }

    private static void SetActiveSafe(GameObject go, bool value)
    {
        if (go != null) go.SetActive(value);
    }
}
