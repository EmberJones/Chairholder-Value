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

        Raycaster.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCursorInput(Vector2 NormPos)
    {
        Vector3 CanvasSpacePosition = new Vector3(CanvasTransform.sizeDelta.x * NormPos.x, CanvasTransform.sizeDelta.y * NormPos.y, 0f);

        PointerEventData MouseEvent = new PointerEventData(EventSystem.current);
        MouseEvent.position = CanvasSpacePosition;
        List<RaycastResult> Results = new List<RaycastResult>();

        Raycaster.Raycast(MouseEvent, Results);
        bool SendMouseDown = Input.GetMouseButtonDown(0);
        bool SendMouseUp = Input.GetMouseButtonUp(0);


        foreach (var result in Results)         // the canvas raycater, hits the button, it's label, the taskbar and the background behind it
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
