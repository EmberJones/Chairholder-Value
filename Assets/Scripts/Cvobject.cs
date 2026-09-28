using UnityEngine;


public class CVObject : DraggableObject
{
    [Header("CV Data")]
    public GeneratedCV Data { get; private set; }

    [Header("Stamp Overlays")]
    [SerializeField] private Sprite approveStampSprite;
    [SerializeField] private Sprite rejectStampSprite;
    [SerializeField] private int decalSortOrderBoost = 1;
    private GameObject _spawnedDecal;
    [SerializeField] private string decalSortingLayerName = "Objects";
    [SerializeField] private Vector3 decalWorldScale = new Vector3(0.15f, 0.15f, 1f);

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
    }

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
    public static event System.Action<CVObject> Stamped;

    public void ApplyStamp(StampType type, Vector3 worldHitPoint)
    {
        if (IsStamped) return;

        _decision = type;
        SpawnStampDecal(type, worldHitPoint);

        if (_audio != null && stampSound != null)
            _audio.PlayOneShot(stampSound);

        GameManager.Instance?.OnCVStamped(this);
        Stamped?.Invoke(this);  
    }
    void SpawnStampDecal(StampType type, Vector3 worldHitPoint)
    {
        Sprite sprite = type == StampType.Approve ? approveStampSprite : rejectStampSprite;
        if (sprite == null) return;

        var decal = new GameObject($"{type}Decal");
        decal.transform.SetParent(transform, worldPositionStays: true);
        decal.transform.position = worldHitPoint;
        decal.transform.rotation = transform.rotation;
        decal.transform.localScale = decalWorldScale;

        var sr = decal.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.flipX = true;
        sr.sortingLayerName = decalSortingLayerName;


        var cvRenderer = GetComponent<SpriteRenderer>();
        sr.sortingOrder = (cvRenderer != null ? cvRenderer.sortingOrder : 0) + decalSortOrderBoost;

        _spawnedDecal = decal;
    }
    public void ClearStamp()
    {
        _decision = null;
        if (_spawnedDecal != null)
        {
            Destroy(_spawnedDecal);
            _spawnedDecal = null;
        }
    }
}