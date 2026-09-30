using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class UIClickSounds : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField, Range(0f, 1f)] private float volume = 0.6f;
    [SerializeField, Range(0f, 0.5f)] private float pitchVariation = 0.05f;

    [Tooltip("How often to look for newly spawned buttons. Set to 0 to disable and call Refresh() manually.")]
    [SerializeField] private float rescanInterval = 0.5f;

    private AudioSource source;
    private float nextScan;

    private readonly HashSet<Button> subscribed = new HashSet<Button>();
    private readonly List<Button> scanBuffer = new List<Button>();

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDisable()
    {
        foreach (Button button in subscribed)
        {
            if (button != null) button.onClick.RemoveListener(PlayClick);
        }
        subscribed.Clear();
    }

    private void Update()
    {
        if (rescanInterval <= 0f || Time.unscaledTime < nextScan) return;
        nextScan = Time.unscaledTime + rescanInterval;
        Refresh();
    }

    public void Refresh()
    {
        subscribed.RemoveWhere(b => b == null); // drop destroyed buttons

        GetComponentsInChildren(true, scanBuffer);
        foreach (Button button in scanBuffer)
        {
            // HashSet.Add returns false if already present, so no duplicate listeners
            if (subscribed.Add(button))
                button.onClick.AddListener(PlayClick);
        }
        scanBuffer.Clear();
    }

    public void PlayClick()
    {
        if (clickSound == null) return;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.PlayOneShot(clickSound, volume);
    }
}
