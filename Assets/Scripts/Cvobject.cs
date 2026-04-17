using System.Collections;
using UnityEngine;

public class CVObject : DraggableObject
{
    [Header("CV Data")]
    [SerializeField] private string cvId;
    public CVData Data { get; private set; }   

    [Header("Stamp Overlays")]
    [SerializeField] private GameObject approveOverlay;  
    [SerializeField] private GameObject rejectOverlay;

    [Header("Stamp Sound")]
    [SerializeField] private AudioClip stampSound;

    [Header("Read Mode Rotation")]
    [SerializeField] private Vector3 deskRotation = new Vector3(60f, 0f, 0f);
    [SerializeField] private Vector3 readRotation = new Vector3(0f, 0f, 0f);

    private StampType? _decision;
    private AudioSource _audio;
    private SpriteRenderer _renderer;

    // Read mode state
    private Vector3 _preReadPosition;
    private Vector3 _preReadScale;
    private int _preReadSortOrder;
    private bool _inReadMode;
    private Coroutine _lerpCoroutine;

    public string CvId => cvId;
    public StampType? Decision => _decision;
    public bool IsStamped => _decision.HasValue;
    public bool InReadMode => _inReadMode;

    protected override void Awake()
    {
        base.Awake();
        _audio = GetComponent<AudioSource>();
        _renderer = GetComponent<SpriteRenderer>();
        TextAsset json = Resources.Load<TextAsset>($"CVs/{cvId}");
        if (json != null)
            SetData(JsonUtility.FromJson<CVData>(json.text));
        else
            Debug.Log($"CV data not found for ID: {cvId}");

        if (approveOverlay != null) approveOverlay.SetActive(false);
        if (rejectOverlay != null) rejectOverlay.SetActive(false);
    }

    public void SetData(CVData data)
    {
        Data = data;
        GetComponentInChildren<CVDisplay>()?.Populate(data);
    }
    // Stamping

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

    // Read mode

    public void EnterReadMode(Vector3 targetPos, float speed, int sortBoost)
    {
        if (_inReadMode) return;
        _inReadMode = true;

        _preReadPosition = transform.position;
        _preReadScale = transform.localScale;

        if (_renderer != null)
        {
            _preReadSortOrder = _renderer.sortingOrder;
            _renderer.sortingOrder = _preReadSortOrder + sortBoost;
        }

        foreach (var child in GetComponentsInChildren<SpriteRenderer>())
        {
            if (child == _renderer) continue;
            child.sortingOrder += sortBoost;
        }

        foreach (var tmp in GetComponentsInChildren<TMPro.TMP_Text>())
        {
            var r = tmp.GetComponent<Renderer>();
            if (r != null) r.sortingOrder += sortBoost;
        }

        if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine);
        _lerpCoroutine = StartCoroutine(
            LerpTo(targetPos, readRotation, _preReadScale * 2.5f, speed));
    }

    public void ExitReadMode(float speed)
    {
        if (!_inReadMode) return;
        _inReadMode = false;

        if (_renderer != null)
        {
            int boost = _renderer.sortingOrder - _preReadSortOrder;
            _renderer.sortingOrder = _preReadSortOrder;

            foreach (var child in GetComponentsInChildren<SpriteRenderer>())
            {
                if (child == _renderer) continue;
                child.sortingOrder -= boost;
            }

            foreach (var tmp in GetComponentsInChildren<TMPro.TMP_Text>())
            {
                var r = tmp.GetComponent<Renderer>();
                if (r != null) r.sortingOrder -= boost;
            }
        }

        if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine);
        _lerpCoroutine = StartCoroutine(
            LerpTo(_preReadPosition, deskRotation, _preReadScale, speed));
    }

    IEnumerator LerpTo(Vector3 targetPos, Vector3 targetEuler, Vector3 targetScale, float speed)
    {
        Quaternion targetRot = Quaternion.Euler(targetEuler);

        while (Vector3.Distance(transform.position, targetPos) > 0.005f
            || Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        {
            float t = Time.deltaTime * speed;
            transform.position = Vector3.Lerp(transform.position, targetPos, t);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, t);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        transform.localScale = targetScale;
    }
}