using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DraggableObject : MonoBehaviour
{

    protected Rigidbody Rb { get; private set; }
    private Vector3 _origin;

    protected virtual void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        Rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;   
        Rb.interpolation = RigidbodyInterpolation.Interpolate;                 
        _origin = transform.position;
    }

    protected virtual void FixedUpdate()
    {
    }

    protected virtual void OnPickedUp() { }
    protected virtual void OnDropped() {}
    protected virtual void OnReturnComplete() { }

    public Vector3 Origin => _origin;
    public void ResetOrigin() => _origin = transform.position;
}