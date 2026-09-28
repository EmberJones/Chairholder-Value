using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class SpeakerProfile
{
    public string speaker = "Boss";

    [Header("Voice")]
    [Range(0.5f, 2f)] public float pitch = 0.8f;
    [Range(0f, 0.5f)] public float pitchVariation = 0.12f;

    [Header("Look")]
    public bool useCustomColours = true;
    public Color frameColour = new Color32(255, 74, 74, 255);
    public Color nameColour = new Color32(31, 10, 10, 255);
    public Color textColour = new Color32(255, 128, 128, 255);
    public Color hintColour = new Color32(255, 74, 74, 255);

    [Header("Entrance")]
    public bool bumpOnStart = true;
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
    [SerializeField] private Image frameImage;
    [SerializeField] private Image nameTabImage;
    [SerializeField] private float slideDistance = 30f;
    [SerializeField] private float hintBlinkSpeed = 0.5f;

    [Header("Default Colours")]
    [SerializeField] private Color defaultFrameColour = new Color32(74, 255, 74, 255);
    [SerializeField] private Color defaultNameColour = new Color32(10, 31, 10, 255);
    [SerializeField] private Color defaultTextColour = new Color32(127, 255, 127, 255);
    [SerializeField] private Color defaultHintColour = new Color32(74, 255, 74, 255);

    [Header("Bump")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip bumpSound;
    [SerializeField, Range(0f, 1f)] private float bumpVolume = 0.8f;
    [SerializeField] private float bumpStrength = 14f;
    [SerializeField] private float bumpDuration = 0.25f;

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

    [Header("Speakers")]
    [SerializeField] private List<SpeakerProfile> speakers = new List<SpeakerProfile>();

    public bool IsPlaying { get; private set; }

    private Vector2 boxHomePosition;
    private TMP_Text hintText;
    private float currentPitch;
    private float currentVariation;

    void Awake()
    {
        Instance = this;
        if (box != null) boxHomePosition = box.anchoredPosition;
        if (continueHint != null) hintText = continueHint.GetComponent<TMP_Text>();
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

        ApplyProfile(FindProfile(seq.lines[0].speaker));

        if (group.alpha < 0.01f) yield return new WaitForSecondsRealtime(openDelay);
        yield return Fade(group.alpha, 1f);

        string lastSpeaker = null;

        foreach (DialogueLine line in seq.lines)
        {
            SpeakerProfile profile = FindProfile(line.speaker);
            ApplyProfile(profile);
            speakerText.text = line.speaker;

            bool newSpeaker = !string.Equals(lastSpeaker, line.speaker, System.StringComparison.OrdinalIgnoreCase);
            lastSpeaker = line.speaker;
            if (newSpeaker && profile != null && profile.bumpOnStart) Bump();

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

    SpeakerProfile FindProfile(string speaker)
    {
        foreach (SpeakerProfile profile in speakers)
        {
            if (string.Equals(profile.speaker, speaker, System.StringComparison.OrdinalIgnoreCase))
                return profile;
        }
        return null;
    }

    void ApplyProfile(SpeakerProfile profile)
    {
        bool custom = profile != null && profile.useCustomColours;

        Color frame = custom ? profile.frameColour : defaultFrameColour;
        if (frameImage != null) frameImage.color = frame;
        if (nameTabImage != null) nameTabImage.color = frame;

        speakerText.color = custom ? profile.nameColour : defaultNameColour;
        bodyText.color = custom ? profile.textColour : defaultTextColour;
        if (hintText != null) hintText.color = custom ? profile.hintColour : defaultHintColour;

        currentPitch = profile != null ? profile.pitch : defaultPitch;
        currentVariation = profile != null ? profile.pitchVariation : defaultPitchVariation;
    }

    void Bump()
    {
        if (sfxSource != null && bumpSound != null) sfxSource.PlayOneShot(bumpSound, bumpVolume);
        if (box != null) StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        float t = 0f;
        while (t < bumpDuration)
        {
            t += SafeDeltaTime();
            float strength = bumpStrength * (1f - t / bumpDuration);
            box.anchoredPosition = boxHomePosition + Random.insideUnitCircle * strength;
            yield return null;
        }
        box.anchoredPosition = boxHomePosition;
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
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && (keyboard.spaceKey.wasPressedThisFrame ||
                                    keyboard.enterKey.wasPressedThisFrame ||
                                    keyboard.numpadEnterKey.wasPressedThisFrame);
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
