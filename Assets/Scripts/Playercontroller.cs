using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask stampLayer;
    [SerializeField] private LayerMask cvLayer;
    [SerializeField] private LayerMask faxLayer;

    [Header("Drag Settings")]
    [SerializeField] private float deskHeight = 0f;
    [SerializeField] private float dragSpring = 15f;
    [SerializeField] private float maxDragSpeed = 20f;
    [SerializeField] private float dragLift = 0.05f;

    [Header("Double-Click")]
    [SerializeField] private float doubleClickThreshold = 0.35f;

    private RigidbodyConstraints _heldOriginalConstraints;
    private Camera _cam;
    private Plane _deskPlane;

    private GameObject _held;
    private Rigidbody _heldRb;
    private Vector3 _grabOffset;
    private bool _heldIsStamp;

    private float _lastClickTime = -999f;
    private GameObject _lastClickTarget;
    private bool _isDragging;

    void Awake()
    {
        _cam = Camera.main;
        _deskPlane = new Plane(Vector3.up, new Vector3(0f, deskHeight, 0f));
    }

    void Update()
    {
        // Middle mouse to force deselect
        if (Input.GetMouseButtonDown(2))
        {
            Deselect();
            return;
        }

        // Left mouse button pressed
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            HandleMouseUp();
        }
    }

    void FixedUpdate()
    {
        if (_heldRb == null || !_isDragging) return;

        Vector3 target = RaycastDeskPoint() + _grabOffset + Vector3.up * dragLift;
        Vector3 vel = (target - _heldRb.position) * dragSpring;
        if (vel.magnitude > maxDragSpeed) vel = vel.normalized * maxDragSpeed;
        _heldRb.linearVelocity = vel;
        _heldRb.angularVelocity = Vector3.zero;
    }

    void HandleMouseDown()
    {
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        // First, do a raycast for everything and get all hits
        RaycastHit[] allHits = Physics.RaycastAll(ray, 100f);

        // Sort by distance
        System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

        // If holding a stamp, try to stamp the first CV we hit
        if (_held != null && _heldIsStamp)
        {
            // Look for a CV to stamp
            foreach (RaycastHit hit in allHits)
            {
                if (((1 << hit.collider.gameObject.layer) & cvLayer) != 0)
                {
                    CVObject cv = hit.collider.GetComponent<CVObject>();
                    StampObject stamp = _held.GetComponent<StampObject>();
                    if (cv != null && stamp != null)
                    {
                        cv.ApplyStamp(stamp.StampType);
                        stamp.PlayStampAnimation();
                        // Stamp is used, deselect it
                        Deselect();
                        return;
                    }
                }
            }
            // If no CV found, just drop the stamp
            Deselect();
            return;
        }

        // If holding something else (non-stamp), drop it
        if (_held != null)
        {
            Deselect();
            return;
        }

        // Nothing held - check what we're clicking on
        foreach (RaycastHit hit in allHits)
        {
            GameObject hitObject = hit.collider.gameObject;
            int layer = hit.collider.gameObject.layer;

            // Check for fax machine (highest priority)
            if (((1 << layer) & faxLayer) != 0)
            {
                hit.collider.GetComponent<FaxMachine>()?.TrySubmitRound();
                return;
            }

            // Check for CV objects
            if (((1 << layer) & cvLayer) != 0)
            {
                float now = Time.unscaledTime;
                bool sameTarget = hitObject == _lastClickTarget;
                bool withinWindow = (now - _lastClickTime) <= doubleClickThreshold;

                if (sameTarget && withinWindow)
                {
                    CVObject cv = hit.collider.GetComponent<CVObject>();
                    if (cv != null)
                    {
                        CVDetailUI.Instance.Open(cv);
                        _lastClickTime = -999f;
                        _lastClickTarget = null;
                        return;
                    }
                }

                _lastClickTime = now;
                _lastClickTarget = hitObject;
                SelectObject(hitObject, isStamp: false);
                return;
            }

            // Check for stamps
            if (((1 << layer) & stampLayer) != 0)
            {
                SelectObject(hitObject, isStamp: true);
                return;
            }
        }
    }

    void HandleMouseUp()
    {
        // Drop the held object when mouse button is released
        if (_held != null && !_heldIsStamp)
        {
            Deselect();
        }
        // If it's a stamp, keep holding it after click
    }

    void SelectObject(GameObject obj, bool isStamp)
    {
        _held = obj;
        _heldIsStamp = isStamp;
        _heldRb = obj.GetComponent<Rigidbody>();
        _grabOffset = obj.transform.position - RaycastDeskPoint();
        _isDragging = true;

        _heldOriginalConstraints = _heldRb.constraints;
        _heldRb.constraints = RigidbodyConstraints.FreezeRotation;

        _held.SendMessage("OnPickedUp", SendMessageOptions.DontRequireReceiver);
    }

    Vector3 RaycastDeskPoint()
    {
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (_deskPlane.Raycast(ray, out float dist))
        {
            return ray.GetPoint(dist);
        }
        return _held != null ? _held.transform.position : Vector3.zero;
    }

    void Deselect()
    {
        if (_held == null) return;

        if (_heldRb != null)
        {
            _heldRb.linearVelocity = Vector3.zero;
            _heldRb.angularVelocity = Vector3.zero;
            _heldRb.constraints = _heldOriginalConstraints;
        }

        _held.SendMessage("OnDropped", SendMessageOptions.DontRequireReceiver);
        _held = null;
        _heldRb = null;
        _heldIsStamp = false;
        _isDragging = false;
    }

    public void ForceDeselect() => Deselect();
    public bool IsHoldingSomething => _held != null;
}