using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    [SerializeField] private bool returnToOriginOnDrop = false;
    [SerializeField] private float returnSpeed = 8f;

    private Vector3 _origin;
    private bool _returning;

    protected virtual void Awake()
    {
        _origin = transform.position;
    }

    protected virtual void Update()
    {
        if (_returning)
        {
            transform.position = Vector3.Lerp(transform.position, _origin, Time.deltaTime * returnSpeed);
            if (Vector3.Distance(transform.position, _origin) < 0.01f)
            {
                transform.position = _origin;
                _returning = false;
                OnReturnComplete();
            }
        }
    }

    // Called by PlayerController via SendMessage
    protected virtual void OnPickedUp()
    {
        _returning = false;
    }

    // Called by PlayerController via SendMessage
    protected virtual void OnDropped()
    {
        if (returnToOriginOnDrop)
            _returning = true;
    }

    // Override in subclass to trigger something when the return animation finishes
    protected virtual void OnReturnComplete() { }

    public Vector3 Origin => _origin;
    public void ResetOrigin() => _origin = transform.position;
}