using System.Collections;
using TMPro;
using UnityEngine;

public class DayTransition : MonoBehaviour
{
    public static DayTransition Instance { get; private set; }

    [SerializeField] private CanvasGroup group;      
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private float fadeTime = 0.6f;
    [SerializeField] private float holdTime = 1.6f;

    public bool IsPlaying { get; private set; }

    void Awake()
    {
        Instance = this;
        group.alpha = 0f;
        group.blocksRaycasts = false;
    }

    public void Play(string title, string subtitle, System.Action onCovered, System.Action onFinished = null)
    {
        if (!IsPlaying) StartCoroutine(Run(title, subtitle, onCovered, onFinished));
    }

    IEnumerator Run(string title, string subtitle, System.Action onCovered, System.Action onFinished)
    {
        IsPlaying = true;
        titleText.text = title;
        subtitleText.text = subtitle;
        group.blocksRaycasts = true;

        yield return Fade(0f, 1f);
        onCovered?.Invoke();
        yield return new WaitForSecondsRealtime(holdTime);
        yield return Fade(1f, 0f);

        group.blocksRaycasts = false;
        IsPlaying = false;
        onFinished?.Invoke();
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        group.alpha = to;
    }
}