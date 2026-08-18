using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private LayerMask cvLayer;

    [Header("Drag Settings")]
    [SerializeField] private float dragZ = -1f;
    [SerializeField] private float smoothSpeed = 20f;
    [SerializeField] private float pickupScaleMultiplier = 1.05f;

    [Header("Double-Click")]
    [SerializeField] private float doubleClickThreshold = 0.35f;

    private Camera _cam;

    // Held state
    private GameObject _held;
    private Vector3 _grabOffset;
    private Vector3 _originalScale;
    private SpriteRenderer _heldRenderer;
    private int _originalSortOrder;
    private bool _heldIsStamp;

    private float _lastClickTime = -999f;
    private GameObject _lastClickTarget;

    // Read mode
    private CVObject _cvInReadMode;

    private const int DragSortOrderBoost = 10;
    private const int ReadModeSortOrderBoost = 20;

    void Awake() => _cam = Camera.main;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (_cvInReadMode != null) ExitReadMode();
            else Deselect();
            return;
        }

        if (Input.GetMouseButtonDown(0))
            HandleLeftClick();

        if (_held != null && _cvInReadMode == null)
            FollowCursor();
    }


    void HandleLeftClick()
    {
        // Any click while in read mode exits it
        if (_cvInReadMode != null)
        {
            ExitReadMode();
            return;
        }

        Vector3 worldPos = MouseWorldPosition(dragZ);
        Collider2D hit = Physics2D.OverlapPoint(worldPos, draggableLayer);


        float now = Time.unscaledTime;
        bool sameTarget = hit != null && hit.gameObject == _lastClickTarget;
        bool withinWindow = (now - _lastClickTime) <= doubleClickThreshold;

        if (sameTarget && withinWindow)
        {
            CVObject cv = hit.GetComponent<CVObject>();
            if (cv != null)
            {
                Deselect();            // Drop if currently held
                _lastClickTime = -999f;
                _lastClickTarget = null;
                EnterReadMode(cv);
                return;
            }
        }

        // Record click for next comparison
        _lastClickTime = now;
        _lastClickTarget = hit != null ? hit.gameObject : null;

        // --- If something is already held ---
        if (_held != null)
        {
            if (_heldIsStamp)
                TryStamp(worldPos);   // Stamp held: try to stamp CV under cursor
            else
                Deselect();           // CV held: left-click drops it
            return;
        }

        if (hit == null) return;
        SelectObject(hit.gameObject, worldPos);
    }

    void SelectObject(GameObject obj, Vector3 worldPos)
    {
        if (_held == obj) return;

        _held = obj;
        _originalScale = _held.transform.localScale;  
        _grabOffset = _held.transform.position - worldPos;
        _heldIsStamp = _held.GetComponent<StampObject>() != null;

        _heldRenderer = _held.GetComponent<SpriteRenderer>();
        if (_heldRenderer != null)
        {
            _originalSortOrder = _heldRenderer.sortingOrder;
            _heldRenderer.sortingOrder = _originalSortOrder + DragSortOrderBoost;
        }

        foreach (var child in _held.GetComponentsInChildren<SpriteRenderer>())
        {
            if (child == _heldRenderer) continue;
            child.sortingOrder += DragSortOrderBoost;
        }

        // Boost child TMP text renderers so they don't get buried
        foreach (var tmp in _held.GetComponentsInChildren<TMPro.TMP_Text>())
        {
            var r = tmp.GetComponent<Renderer>();
            if (r != null) r.sortingOrder += DragSortOrderBoost;
        }

        _held.transform.localScale = _originalScale * pickupScaleMultiplier;
        _held.SendMessage("OnPickedUp", SendMessageOptions.DontRequireReceiver);
    }

    void TryStamp(Vector3 worldPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(worldPos, cvLayer);

        if (hit != null)
        {
            CVObject cv = hit.GetComponent<CVObject>();
            StampObject stamp = _held.GetComponent<StampObject>();
            if (cv != null && stamp != null)
                cv.ApplyStamp(stamp.StampType);
        }

        Deselect();
    }

    void FollowCursor()
    {
        Vector3 target = MouseWorldPosition(dragZ) + _grabOffset;
        _held.transform.position = Vector3.Lerp(
            _held.transform.position, target, Time.deltaTime * smoothSpeed);
    }

    void Deselect()
    {
        if (_held == null) return;

        _held.transform.localScale = _originalScale;

        if (_heldRenderer != null)
            _heldRenderer.sortingOrder = _originalSortOrder;

        // Restore child SpriteRenderer sort orders
        foreach (var child in _held.GetComponentsInChildren<SpriteRenderer>())
        {
            if (child == _heldRenderer) continue;
            child.sortingOrder -= DragSortOrderBoost;
        }

        // Restore child TMP sort orders
        foreach (var tmp in _held.GetComponentsInChildren<TMPro.TMP_Text>())
        {
            var r = tmp.GetComponent<Renderer>();
            if (r != null) r.sortingOrder -= DragSortOrderBoost;
        }

        _held.SendMessage("OnDropped", SendMessageOptions.DontRequireReceiver);

        _held = null;
        _heldRenderer = null;
        _heldIsStamp = false;
    }

    void EnterReadMode(CVObject cv)
    {
        _cvInReadMode = cv;
        CVDetailUI.Instance.Open(cv);
    }

    void ExitReadMode()
    {
        if (_cvInReadMode == null) return;
        CVDetailUI.Instance.Close();
        _cvInReadMode = null;
        _lastClickTarget = null;
    }


    Vector3 MouseWorldPosition(float z)
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = _cam.transform.position.z * -1f + z;
        return _cam.ScreenToWorldPoint(mouse);
    }

    public void ForceDeselect()
    {
        if (_cvInReadMode != null) ExitReadMode();
        Deselect();
    }

    public bool IsHoldingSomething => _held != null;
    public bool IsInReadMode => _cvInReadMode != null;
    public GameObject HeldObject => _held;
}