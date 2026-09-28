using UnityEngine;

public class CVDetailUI : MonoBehaviour
{
    public static CVDetailUI Instance { get; private set; }
    public static event System.Action<CVObject> CVOpened;
    [SerializeField] private GameObject Panel;
    [SerializeField] private CVDisplay Display;

    private CVObject _currentCV;

    private void Awake()
    {
        Instance = this;
        Panel.SetActive(false);
    }

    public void Open(CVObject cv)
    {
        _currentCV = cv;
        Display.Populate(cv.Data);
        Panel.SetActive(true);
        CVOpened?.Invoke(cv);
    }

    public void Close()
    {
        _currentCV = null;
        Panel.SetActive(false);
    }
}