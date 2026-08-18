using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLookController : MonoBehaviour
{
    [Header("Sensitivity")]
    [Tooltip("Mouse look sensitivity.")]
    [Range(0.01f, 10f)]
    public float sensitivity = 2f;

    [Header("Yaw Clamp (left/right, degrees)")]
    public float minYaw = -90f;
    public float maxYaw = 90f;

    [Header("Pitch Clamp (up/down, degrees)")]
    public float minPitch = -45f;
    public float maxPitch = 45f;

    private float yaw;
    private float pitch;

    private InputAction lookAction;
    private InputAction rightClickAction;

    private void Awake()
    {
        lookAction = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta");
        rightClickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/rightButton");
    }

    private void OnEnable()
    {
        lookAction.Enable();
        rightClickAction.Enable();

        Vector3 euler = transform.localEulerAngles;
        yaw = NormalizeAngle(euler.y);
        pitch = NormalizeAngle(euler.x);        // normalizing the angle to prevent snapping

        yaw = Mathf.Clamp(yaw, minYaw, maxYaw);         // if we start out of angle, we fix it now, so the player doesn't experience any snapping when they first try to control the camera
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.localEulerAngles = new Vector3(pitch, yaw, 0f);
    }

    private void OnDisable()
    {
        lookAction.Disable();
        rightClickAction.Disable();
    }

    private void Update()
    {
                        // Only rotate while RMB is held
        if (!rightClickAction.IsPressed())
            return;

        Vector2 delta = lookAction.ReadValue<Vector2>();
        if (delta == Vector2.zero)
            return;

        yaw += delta.x * sensitivity * 0.1f;
        pitch -= delta.y * sensitivity * 0.1f;

        yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localEulerAngles = new Vector3(pitch, yaw, 0f);
    }

    private static float NormalizeAngle(float angle)
    {
        if (angle > 180f)           
            angle -= 360f;          // converting 0 to 360 degree rotation too -180 to 180, to account for negative angles aswell
        return angle;
    }
}
