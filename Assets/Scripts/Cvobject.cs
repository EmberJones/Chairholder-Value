using UnityEngine;

public class CVObject : DraggableObject
{
    [Header("CV Data")]
    [SerializeField] private string cvId;   // Matches the "id" field in the JSON file

    [Header("Stamp Overlays")]
    [SerializeField] private GameObject approveOverlay;  // Green APPROVED stamp sprite, starts inactive
    [SerializeField] private GameObject rejectOverlay;   // Red REJECTED stamp sprite, starts inactive

    [Header("Stamp Sound")]
    [SerializeField] private AudioClip stampSound;

    private StampType? _decision;
    private AudioSource _audio;

    public string CvId => cvId;
    public StampType? Decision => _decision;
    public bool IsStamped => _decision.HasValue;

    protected override void Awake()
    {
        base.Awake();
        _audio = GetComponent<AudioSource>();

        // Make sure overlays are hidden at start
        if (approveOverlay != null) approveOverlay.SetActive(false);
        if (rejectOverlay != null) rejectOverlay.SetActive(false);
    }

    public void ApplyStamp(StampType type)
    {
        // Prevent re-stamping an already-stamped CV
        if (IsStamped) return;

        _decision = type;

        if (type == StampType.Approve)
        {
            if (approveOverlay != null) approveOverlay.SetActive(true);
        }
        else
        {
            if (rejectOverlay != null) rejectOverlay.SetActive(true);
        }

        if (_audio != null && stampSound != null)
            _audio.PlayOneShot(stampSound);

        // Notify GameManager (or any listener) that a decision was made
       // GameManager.Instance?.OnCVStamped(this);
    }

    public void ClearStamp()
    {
        _decision = null;
        if (approveOverlay != null) approveOverlay.SetActive(false);
        if (rejectOverlay != null) rejectOverlay.SetActive(false);
    }
}