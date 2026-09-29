using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private GameObject continueIndicator;   

    [Header("Typewriter")]
    [SerializeField] private float charsPerSecond = 40f;

    [Header("Blip Audio (optional, Simlish-style)")]
    [SerializeField] private AudioSource blipSource;
    [SerializeField] private AudioClip[] blipClips;
    [SerializeField] private int charsPerBlip = 2;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.15f);

    public bool IsPlaying { get; private set; }

    public bool BlocksInput => IsPlaying || Time.frameCount <= _closedFrame;

    private int _closedFrame = -1;
    private int _blipCounter;
    private Coroutine _routine;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Play(DialogueSequence sequence, System.Action onComplete = null,
                     Dictionary<string, string> tokens = null)
    {
        if (sequence == null || sequence.lines.Count == 0) { onComplete?.Invoke(); return; }
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Run(sequence, onComplete, tokens));
    }

    IEnumerator Run(DialogueSequence sequence, System.Action onComplete, Dictionary<string, string> tokens)
    {
        IsPlaying = true;
        panel.SetActive(true);

        foreach (var line in sequence.lines)
        {
            speakerText.text = line.speaker;
            yield return TypeLine(ApplyTokens(line.text, tokens));
            yield return null;  

            if (continueIndicator != null) continueIndicator.SetActive(true);
            while (!AdvancePressed()) yield return null;
            if (continueIndicator != null) continueIndicator.SetActive(false);
        }

        panel.SetActive(false);
        IsPlaying = false;
        _closedFrame = Time.frameCount;
        _routine = null;
        onComplete?.Invoke();
    }

    IEnumerator TypeLine(string text)
    {
        bodyText.text = text;
        bodyText.maxVisibleCharacters = 0;
        bodyText.ForceMeshUpdate();
        int total = bodyText.textInfo.characterCount;
        float shown = 0f;

        yield return null;   

        while (bodyText.maxVisibleCharacters < total)
        {
            if (AdvancePressed()) break;

            shown += charsPerSecond * Time.unscaledDeltaTime;
            int target = Mathf.Min(total, Mathf.FloorToInt(shown));

            for (int i = bodyText.maxVisibleCharacters; i < target; i++)
            {
                char c = bodyText.textInfo.characterInfo[i].character;
                if (!char.IsWhiteSpace(c) && !char.IsPunctuation(c) && ++_blipCounter % charsPerBlip == 0)
                    PlayBlip();
            }

            bodyText.maxVisibleCharacters = target;
            yield return null;
        }

        bodyText.maxVisibleCharacters = total;
    }

    void PlayBlip()
    {
        if (blipSource == null || blipClips == null || blipClips.Length == 0) return;
        blipSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        blipSource.PlayOneShot(blipClips[Random.Range(0, blipClips.Length)]);
    }

    static bool AdvancePressed() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);

    static string ApplyTokens(string text, Dictionary<string, string> tokens)
    {
        if (tokens == null) return text;
        foreach (var kv in tokens) text = text.Replace("{" + kv.Key + "}", kv.Value);
        return text;
    }
}