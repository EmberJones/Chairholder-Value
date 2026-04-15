using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private LayerMask cvLayer;          // Separate layer for CV objects only
    [SerializeField] private float dragZ = -1f;
    [SerializeField] private float smoothSpeed = 20f;
    [SerializeField] private float pickupScaleMultiplier = 1.05f;

    private Camera _cam;
    private GameObject _held;
    private Vector3 _grabOffset;
    private Vector3 _originalScale;
    private int _originalSortOrder;
    private SpriteRenderer _heldRenderer;
    private bool _heldIsStamp;

    private const int DragSortOrderBoost = 10;

    void Awake()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        // Right-click always deselects, regardless of what is held
        if (Input.GetMouseButtonDown(1))
        {
            Deselect();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (_held == null)
            {
                // Nothing held — try to select whatever is under the cursor
                TrySelect();
            }
            else if (_heldIsStamp)
            {
                // Stamp held — left-click tries to stamp a CV, or deselects if none found
                TryStamp();
            }
            // Non-stamp held: left-click does nothing; right-click (above) deselects
        }

        if (_held != null)
            FollowCursor();
    }


    void TrySelect()
    {
        Vector3 worldPos = MouseWorldPosition(dragZ);
        Collider2D hit = Physics2D.OverlapPoint(worldPos, draggableLayer);

        if (hit == null) return;

        _held = hit.gameObject;
        _originalScale = _held.transform.localScale;
        _grabOffset = _held.transform.position - worldPos;
        _heldIsStamp = _held.GetComponent<StampObject>() != null;

        _heldRenderer = _held.GetComponent<SpriteRenderer>();
        if (_heldRenderer != null)
        {
            _originalSortOrder = _heldRenderer.sortingOrder;
            _heldRenderer.sortingOrder = _originalSortOrder + DragSortOrderBoost;
        }

        _held.transform.localScale = _originalScale * pickupScaleMultiplier;
        _held.SendMessage("OnPickedUp", SendMessageOptions.DontRequireReceiver);
    }

    void TryStamp()
    {
        // Raycast on the CV layer specifically — stamps live on draggableLayer,
        // so using cvLayer here means we never accidentally stamp the stamp itself
        Vector3 worldPos = MouseWorldPosition(dragZ);
        Collider2D hit = Physics2D.OverlapPoint(worldPos, cvLayer);

        if (hit != null)
        {
            CVObject cv = hit.GetComponent<CVObject>();
            StampObject stamp = _held.GetComponent<StampObject>();

            if (cv != null && stamp != null)
                cv.ApplyStamp(stamp.StampType);

            // Always deselect after a stamp action so the player must
            // deliberately re-select to stamp again — prevents accidental double-stamps
            Deselect();
        }
        else
        {
            // Clicked on empty space with no CV underneath — deselect
            Deselect();
        }
    }

    void FollowCursor()
    {
        Vector3 target = MouseWorldPosition(dragZ) + _grabOffset;

        _held.transform.position = Vector3.Lerp(
            _held.transform.position,
            target,
            Time.deltaTime * smoothSpeed
        );
    }

    void Deselect()
    {
        if (_held == null) return;

        _held.transform.localScale = _originalScale;

        if (_heldRenderer != null)
            _heldRenderer.sortingOrder = _originalSortOrder;

        _held.SendMessage("OnDropped", SendMessageOptions.DontRequireReceiver);

        _held = null;
        _heldRenderer = null;
        _heldIsStamp = false;
    }

    

    Vector3 MouseWorldPosition(float z)
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = _cam.transform.position.z * -1f + z;
        return _cam.ScreenToWorldPoint(mouse);
    }


    public void ForceDeselect() => Deselect();
    public bool IsHoldingSomething => _held != null;
    public GameObject HeldObject => _held;
}