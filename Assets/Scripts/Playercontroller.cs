using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask stampLayer;
    [SerializeField] private LayerMask cvLayer;
    [SerializeField] private LayerMask faxLayer; 
    [SerializeField] private LayerMask propLayer;

    [Header("Drag Settings")]
    [SerializeField] private float deskHeight = 0f;
    [SerializeField] private float dragSpring = 15f;
    [SerializeField] private float maxDragSpeed = 20f;
    [SerializeField] private float dragLift = 0.05f;

    [Header("Double-Click")]
    [SerializeField] private float doubleClickThreshold = 0.35f;

    private Quaternion _heldUprightRotation;  

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
    bool InputBlocked =>
    (DialogueUI.Instance != null && DialogueUI.Instance.BlocksInput) ||
    (DayTransition.Instance != null && DayTransition.Instance.IsPlaying);


    void Update()
    {
        if (InputBlocked) return;
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

        if (_heldIsStamp)
            _heldRb.rotation = _heldUprightRotation;
    }

    void HandleMouseDown()
    {
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        RaycastHit[] allHits = Physics.RaycastAll(ray, 100f);

        System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

        if (_held != null && _heldIsStamp)
        {
            foreach (RaycastHit hit in allHits)
            {
                if (((1 << hit.collider.gameObject.layer) & cvLayer) != 0)
                {
                    CVObject cv = hit.collider.GetComponent<CVObject>();
                    StampObject stamp = _held.GetComponent<StampObject>();
                    if (cv != null && stamp != null)
                    {
                        Vector3 stampPos = _held.transform.position;
                        cv.ApplyStamp(stamp.StampType, _held.transform.position);
                        stamp.PlayStampAnimation();
                        Deselect();
                        return;
                    }
                }
            }
            Deselect();
            return;
        }

        if (_held != null)
        {
            Deselect();
            return;
        }

        foreach (RaycastHit hit in allHits)
        {
            GameObject hitObject = hit.collider.gameObject;
            int layer = hit.collider.gameObject.layer;

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
            if (((1 << layer) & propLayer) != 0)
            {
                SelectObject(hitObject, isStamp: false);
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
    }

    void SelectObject(GameObject obj, bool isStamp)
    {
        _held = obj;
        _heldIsStamp = isStamp;
        _heldRb = obj.GetComponent<Rigidbody>();
        _grabOffset = isStamp ? Vector3.zero : obj.transform.position - RaycastDeskPoint();
        _isDragging = true;

        _heldOriginalConstraints = _heldRb.constraints;

        if (isStamp)
        {
            StampObject stampObj = obj.GetComponent<StampObject>();
            _heldUprightRotation = stampObj != null ? stampObj.UprightRotation : Quaternion.identity;
            _heldRb.constraints = RigidbodyConstraints.None;   
            _heldRb.rotation = _heldUprightRotation;          
        }

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