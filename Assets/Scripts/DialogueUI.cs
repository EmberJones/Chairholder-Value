using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class SpeakerVoice
{
    public string speaker = "Boss";
    [Range(0.5f, 2f)] public float pitch = 0.8f;
    [Range(0f, 0.5f)] public float pitchVariation = 0.12f;
}

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private GameObject continueHint;

    [Header("Style")]
    [SerializeField] private RectTransform box;
    [SerializeField] private float slideDistance = 30f;
    [SerializeField] private float hintBlinkSpeed = 0.5f;

    [Header("Timing")]
    [SerializeField] private float fadeTime = 0.25f;
    [SerializeField] private float openDelay = 0.5f;
    [SerializeField] private float charactersPerSecond = 40f;
    [SerializeField] private float sentencePause = 0.25f;
    [SerializeField] private float commaPause = 0.1f;

    [Header("Voice")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip[] voiceClips;
    [SerializeField, Range(0f, 1f)] private float voiceVolume = 0.7f;
    [SerializeField] private int soundEveryNLetters = 1;
    [SerializeField] private float defaultPitch = 1f;
    [SerializeField] private float defaultPitchVariation = 0.12f;
    [SerializeField] private List<SpeakerVoice> speakerVoices = new List<SpeakerVoice>();

    public bool IsPlaying { get; private set; }

    private Vector2 boxHomePosition;
    private float currentPitch;
    private float currentVariation;

    void Awake()
    {
        Instance = this;
        if (box != null) boxHomePosition = box.anchoredPosition;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        if (continueHint != null) continueHint.SetActive(false);
    }

    public void Play(DialogueSequence seq, System.Action onDone, Dictionary<string, string> tokens = null)
    {
        if (seq == null || seq.lines == null || seq.lines.Count == 0)
        {
            onDone?.Invoke();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(Run(seq, onDone, tokens));
    }

    IEnumerator Run(DialogueSequence seq, System.Action onDone, Dictionary<string, string> tokens)
    {
        IsPlaying = true;
        group.blocksRaycasts = true;
        if (group.alpha < 0.01f) yield return new WaitForSecondsRealtime(openDelay);
        yield return Fade(group.alpha, 1f);

        foreach (DialogueLine line in seq.lines)
        {
            speakerText.text = line.speaker;
            SetVoiceFor(line.speaker);
            yield return TypeLine(ApplyTokens(line.text, tokens));

            if (continueHint != null) continueHint.SetActive(true);
            yield return WaitForContinue();
            if (continueHint != null) continueHint.SetActive(false);
        }

        yield return Fade(1f, 0f);
        group.blocksRaycasts = false;
        IsPlaying = false;
        onDone?.Invoke();
    }

    IEnumerator TypeLine(string text)
    {
        bodyText.text = text;
        bodyText.maxVisibleCharacters = 0;
        bodyText.ForceMeshUpdate();
        TMP_TextInfo info = bodyText.textInfo;
        int total = info.characterCount;

        int letterCount = 0;
        float timer = 0f;
        yield return null;

        for (int i = 0; i < total; i++)
        {
            bodyText.maxVisibleCharacters = i + 1;
            char c = info.characterInfo[i].character;

            if (char.IsLetterOrDigit(c))
            {
                if (letterCount % Mathf.Max(1, soundEveryNLetters) == 0) PlayVoice(c);
                letterCount++;
            }

            float wait = 1f / Mathf.Max(1f, charactersPerSecond);
            if (c == '.' || c == '!' || c == '?') wait += sentencePause;
            else if (c == ',') wait += commaPause;

            while (timer < wait)
            {
                if (ContinuePressed())
                {
                    bodyText.maxVisibleCharacters = total;
                    yield return null;
                    yield break;
                }
                timer += SafeDeltaTime();
                yield return null;
            }
            timer -= wait;
        }
    }

    void SetVoiceFor(string speaker)
    {
        currentPitch = defaultPitch;
        currentVariation = defaultPitchVariation;

        foreach (SpeakerVoice voice in speakerVoices)
        {
            if (string.Equals(voice.speaker, speaker, System.StringComparison.OrdinalIgnoreCase))
            {
                currentPitch = voice.pitch;
                currentVariation = voice.pitchVariation;
                return;
            }
        }
    }

    void PlayVoice(char c)
    {
        if (voiceSource == null || voiceClips == null || voiceClips.Length == 0) return;

        c = char.ToLowerInvariant(c);

        int index = "aeiou".IndexOf(c);
        if (index < 0 || index >= voiceClips.Length) index = c % voiceClips.Length;

        float step = ((c * 7) % 5 - 2) / 2f;

        voiceSource.pitch = currentPitch * (1f + currentVariation * step);
        voiceSource.Stop();
        voiceSource.PlayOneShot(voiceClips[index], voiceVolume);
    }

    IEnumerator WaitForContinue()
    {
        yield return null;
        float timer = 0f;
        while (!ContinuePressed())
        {
            timer += Time.unscaledDeltaTime;
            if (continueHint != null && timer >= hintBlinkSpeed)
            {
                timer = 0f;
                continueHint.SetActive(!continueHint.activeSelf);
            }
            yield return null;
        }
    }

    static bool ContinuePressed()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;
        return (mouse != null && mouse.leftButton.wasPressedThisFrame) ||
               (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame ||
                                     keyboard.enterKey.wasPressedThisFrame));
    }

    static string ApplyTokens(string text, Dictionary<string, string> tokens)
    {
        if (string.IsNullOrEmpty(text) || tokens == null) return text;
        foreach (var pair in tokens)
            text = text.Replace("{" + pair.Key + "}", pair.Value);
        return text;
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += SafeDeltaTime();
            group.alpha = Mathf.Lerp(from, to, t / fadeTime);
            SlideBox(group.alpha);
            yield return null;
        }
        group.alpha = to;
        SlideBox(to);
    }

    static float SafeDeltaTime() => Mathf.Min(Time.unscaledDeltaTime, 0.05f);

    void SlideBox(float visibility)
    {
        if (box == null) return;
        box.anchoredPosition = boxHomePosition + Vector2.down * slideDistance * (1f - visibility);
    }
}
