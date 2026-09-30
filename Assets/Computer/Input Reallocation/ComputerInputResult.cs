using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ComputerInputResult : MonoBehaviour
{

    [SerializeField] RectTransform CanvasTransform;
    GraphicRaycaster Raycaster; // already on the canvas

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Raycaster = GetComponent<GraphicRaycaster>();

        Raycaster.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCursorInput(Vector2 NormPos)
    {
        Canvas canvas = CanvasTransform.GetComponentInParent<Canvas>();
        Camera targetCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;

        // 1. Convert normalized UV (0..1) to Canvas Local Position centered on Pivot
        Vector2 canvasSize = CanvasTransform.rect.size;
        Vector2 localPoint = new Vector2(
            (NormPos.x - CanvasTransform.pivot.x) * canvasSize.x,
            (NormPos.y - CanvasTransform.pivot.y) * canvasSize.y
        );

        // 2. Convert Canvas Local Position -> World Position -> Screen Pixel Position
        Vector3 worldPoint = CanvasTransform.TransformPoint(localPoint);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(targetCamera, worldPoint);

        // 3. Populate PointerEventData with valid Screen Coordinates
        PointerEventData MouseEvent = new PointerEventData(EventSystem.current)
        {
            position = screenPoint
        };

        List<RaycastResult> Results = new List<RaycastResult>();
        Raycaster.Raycast(MouseEvent, Results);

        bool SendMouseDown = Input.GetMouseButtonDown(0);
        bool SendMouseUp = Input.GetMouseButtonUp(0);

        foreach (var result in Results)
        {
            if (SendMouseDown)
            {
                ExecuteEvents.Execute(result.gameObject, MouseEvent, ExecuteEvents.pointerDownHandler);
            }
            else if (SendMouseUp)
            {
                ExecuteEvents.Execute(result.gameObject, MouseEvent, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(result.gameObject, MouseEvent, ExecuteEvents.pointerClickHandler);
            }
        }
    }
}
