using UnityEngine;
using static UnityEngine.UI.Image;


public class CVObject : DraggableObject
{
    [Header("CV Data")]
    public GeneratedCV Data { get; private set; }

    [Header("Stamp Decals")]
    [SerializeField] private Sprite approveStampSprite;
    [SerializeField] private Sprite rejectStampSprite;
    [SerializeField] private int decalSortOrderBoost = 1;
    [SerializeField] private string decalSortingLayerName = "Objects";
    [SerializeField] private Vector3 decalWorldScale = new Vector3(0.15f, 0.15f, 1f);
    private GameObject _spawnedDecal;

    [SerializeField] private Vector2 localHalfSize = new Vector2(2f, 2.5f);

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

        gameObject.GetComponentInChildren<SpriteRenderer>().sprite = Data.FormatProfile.DeskSprite;

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
        Vector3 clampedPoint = ClampToCVBounds(worldHitPoint);
        var decal = new GameObject($"{type}Decal");
        decal.transform.SetParent(transform, worldPositionStays: true);
        decal.transform.position = clampedPoint;
        decal.transform.rotation = transform.rotation;
        decal.transform.localScale = decalWorldScale;


        Vector3 lp = decal.transform.localPosition;
        lp.z = 0f;
        decal.transform.localPosition = lp;

        var sr = decal.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.flipX = true;
        sr.sortingLayerName = decalSortingLayerName;

        var cvRenderer = GetComponent<SpriteRenderer>();
        sr.sortingOrder = (cvRenderer != null ? cvRenderer.sortingOrder : 0) + decalSortOrderBoost;

        _spawnedDecal = decal;

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
    Vector3 ClampToCVBounds(Vector3 worldPoint)
    {
        Vector3 local = transform.InverseTransformPoint(worldPoint);

        local.x = Mathf.Clamp(local.x, -localHalfSize.x, localHalfSize.x);
        local.y = Mathf.Clamp(local.y, -localHalfSize.y, localHalfSize.y);
        local.z = 0f;

        return transform.TransformPoint(local);
    }
}