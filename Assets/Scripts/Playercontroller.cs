using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private float dragZ = -1f;         
    [SerializeField] private float smoothSpeed = 20f;   
    [SerializeField] private float pickupScaleMultiplier = 1.05f;

    private Camera _cam;
    private GameObject _held;
    private Vector3 _grabOffset;     
    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private int _originalSortOrder;
    private SpriteRenderer _heldRenderer;

    private const int DragSortOrderBoost = 10;

    void Awake()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryPickUp();
        if (Input.GetMouseButtonUp(0)) Drop();
        if (_held != null) DragHeld();
    }


    void TryPickUp()
    {
        Vector3 worldPos = MouseWorldPosition(dragZ);
        Collider2D hit = Physics2D.OverlapPoint(worldPos, draggableLayer);

        if (hit == null) return;

        _held = hit.gameObject;
        _originalPosition = _held.transform.position;
        _originalScale = _held.transform.localScale;

        _grabOffset = _held.transform.position - worldPos;

        _heldRenderer = _held.GetComponent<SpriteRenderer>();
        if (_heldRenderer != null)
        {
            _originalSortOrder = _heldRenderer.sortingOrder;
            _heldRenderer.sortingOrder = _originalSortOrder + DragSortOrderBoost;
        }

        _held.transform.localScale = _originalScale * pickupScaleMultiplier;

        _held.SendMessage("OnPickedUp", SendMessageOptions.DontRequireReceiver);
    }

    void DragHeld()
    {
        Vector3 target = MouseWorldPosition(dragZ) + _grabOffset;

        _held.transform.position = Vector3.Lerp(
            _held.transform.position,
            target,
            Time.deltaTime * smoothSpeed
        );
    }

    void Drop()
    {
        if (_held == null) return;

        _held.transform.localScale = _originalScale;
        if (_heldRenderer != null)
            _heldRenderer.sortingOrder = _originalSortOrder;

        _held.SendMessage("OnDropped", SendMessageOptions.DontRequireReceiver);

        _held = null;
        _heldRenderer = null;
    }


    Vector3 MouseWorldPosition(float z)
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = _cam.transform.position.z * -1f + z;
        return _cam.ScreenToWorldPoint(mouse);
    }



    public void ForceDropHeld() => Drop();
    public bool IsHoldingSomething => _held != null;
    public GameObject HeldObject => _held;
}