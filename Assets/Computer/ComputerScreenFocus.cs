using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ComputerScreenFocus : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform FocusPoint;

    [SerializeField] private Collider ScreenCollider;

    [Tooltip("Defaults to Camera.main.")]
    [SerializeField] private Camera TargetCamera;

    [Header("Settings")]
    [SerializeField] private float ScreenFov = 35f;
    [SerializeField] private float TransitionDuration = 0.6f;
    [SerializeField] private AnimationCurve Easing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float MaxClickDistance = 3f;
    [SerializeField] private LayerMask ScreenLayer = ~0;
    [SerializeField] private KeyCode ExitKey = KeyCode.Escape;

    [Header("Disabled while focused")]
    [SerializeField] private Behaviour[] DisableWhileFocused;

    private enum State { Idle, MovingIn, Focused, MovingOut }
    private State state = State.Idle;

    // Saved camera state, restored on exit
    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private float savedFov;
    private CursorLockMode savedLockMode;
    private bool savedCursorVisible;

    private Coroutine moveRoutine;

    private void Awake()
    {
        if (TargetCamera == null) TargetCamera = Camera.main;
        if (ScreenCollider == null) ScreenCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                if (Input.GetMouseButtonDown(0) && ClickedOnScreen())
                    EnterFocus();
                break;

            case State.Focused:
                if (Input.GetKeyDown(ExitKey))
                    ExitFocus();
                break;
        }
    }

    private bool ClickedOnScreen()
    {
        // When the cursor is locked (FPS), click from the centre of the screen instead of the mouse position.
        Vector3 screenPos = Cursor.lockState == CursorLockMode.Locked ? new Vector3(Screen.width * 0.5f, Screen.height * 0.5f) : Input.mousePosition;

        Ray ray = TargetCamera.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out RaycastHit hit, MaxClickDistance, ScreenLayer, QueryTriggerInteraction.Ignore) && hit.collider == ScreenCollider;
    }

    private void EnterFocus()
    {
        // Save where we were
        savedPosition = TargetCamera.transform.position;
        savedRotation = TargetCamera.transform.rotation;
        savedFov = TargetCamera.fieldOfView;
        savedLockMode = Cursor.lockState;
        savedCursorVisible = Cursor.visible;

        SetPlayerControl(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        state = State.MovingIn;

        ComputerInputSource.Instance.Active = true;
        StartMove(FocusPoint.position, FocusPoint.rotation, ScreenFov, State.Focused);
    }

    private void ExitFocus()
    {
        state = State.MovingOut;
        StartMove(savedPosition, savedRotation, savedFov, State.Idle, onDone: () =>
        {
            Cursor.lockState = savedLockMode;
            Cursor.visible = savedCursorVisible;
            SetPlayerControl(true);
        });
        ComputerInputSource.Instance.Active = false;
    }

    private void SetPlayerControl(bool enabled)
    {
        foreach (var b in DisableWhileFocused)
            if (b != null) b.enabled = enabled;
    }

    private void StartMove(Vector3 pos, Quaternion rot, float fov, State endState, System.Action onDone = null)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(MoveCamera(pos, rot, fov, endState, onDone));
    }

    private IEnumerator MoveCamera(Vector3 toPos, Quaternion toRot, float toFov, State endState, System.Action onDone)
    {
        Transform cam = TargetCamera.transform;
        Vector3 fromPos = cam.position;
        Quaternion fromRot = cam.rotation;
        float fromFov = TargetCamera.fieldOfView;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.01f, TransitionDuration);
            float k = Easing.Evaluate(Mathf.Clamp01(t));

            cam.SetPositionAndRotation( Vector3.LerpUnclamped(fromPos, toPos, k), Quaternion.SlerpUnclamped(fromRot, toRot, k));
            TargetCamera.fieldOfView = Mathf.LerpUnclamped(fromFov, toFov, k);

            yield return null;
        }

        cam.SetPositionAndRotation(toPos, toRot);
        TargetCamera.fieldOfView = toFov;

        state = endState;
        moveRoutine = null;
        onDone?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        if (FocusPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(FocusPoint.position, 0.05f);
        Gizmos.DrawRay(FocusPoint.position, FocusPoint.forward * 0.5f);
    }
}
