using UnityEngine;


public class CVObject : DraggableObject
{
    [Header("CV Data")]
    public GeneratedCV Data { get; private set; }

    [Header("Stamp Overlays")]
    [SerializeField] private GameObject approveOverlay;
    [SerializeField] private GameObject rejectOverlay;

    [Header("Stamp Sound")]
    [SerializeField] private AudioClip stampSound;

    private StampType? _decision;
    private AudioSource _audio;

    public StampType? Decision => _decision;
    public bool IsStamped => _decision.HasValue;

    protected override void Awake()
    {
        base.Awake();
        _audio = GetComponent<AudioSource>();

        if (approveOverlay != null) approveOverlay.SetActive(false);
        if (rejectOverlay != null) rejectOverlay.SetActive(false);
    }

    // Called by whatever spawns the batch
    public void SetData(GeneratedCV data)
    {
        Data = data;

        gameObject.GetComponent<SpriteRenderer>().sprite = Data.FormatProfile.DeskSprite;

    }

    protected override void OnPickedUp()
    {
        base.OnPickedUp();
    }

    protected override void OnDropped()
    {
        base.OnDropped();
    }

    // Stamping - unchanged behaviour, still happens on the desk object

    public void ApplyStamp(StampType type)
    {
        if (IsStamped) return;

        _decision = type;

        if (type == StampType.Approve)
        { if (approveOverlay != null) approveOverlay.SetActive(true); }
        else
        { if (rejectOverlay != null) rejectOverlay.SetActive(true); }

        if (_audio != null && stampSound != null)
            _audio.PlayOneShot(stampSound);

        GameManager.Instance?.OnCVStamped(this);
    }

    public void ClearStamp()
    {
        _decision = null;
        if (approveOverlay != null) approveOverlay.SetActive(false);
        if (rejectOverlay != null) rejectOverlay.SetActive(false);
    }
}