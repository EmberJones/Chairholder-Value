using UnityEngine;

/// <summary>
/// Handles double-click-to-select: raycasts from the camera when two clicks
/// land on the same object within a short window, enables the Outline
/// component (see Outline.cs) on it, and disables it on the previously
/// selected object. A single click alone does nothing (so it doesn't
/// conflict with a separate single-click interaction you may already have).
///
/// Add this to any single object in the scene (e.g. an empty "GameManager").
/// Each selectable object needs an Outline component (it can start disabled)
/// and a Collider so the raycast can hit it.
/// </summary>
public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Camera selectionCamera;
    [SerializeField] private LayerMask selectableLayers = ~0; // everything by default
    [Tooltip("Max time in seconds between two clicks on the same object for it to count as a double-click.")]
    [SerializeField] private float doubleClickThreshold = 0.3f;

    private Outline currentlySelected;
    private Outline lastClickedOutline;
    private float lastClickTime = -1f;

    private void Awake()
    {
        if (selectionCamera == null)
        {
            selectionCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        Ray ray = selectionCamera.ScreenPointToRay(Input.mousePosition);
        Outline hitOutline = null;

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, selectableLayers))
        {
            hitOutline = hit.collider.GetComponentInParent<Outline>();
        }

        bool isDoubleClick = hitOutline != null
            && hitOutline == lastClickedOutline
            && (Time.time - lastClickTime) <= doubleClickThreshold;

        if (isDoubleClick)
        {
            Select(hitOutline);
            // Reset so a third rapid click doesn't chain into another "double-click".
            lastClickedOutline = null;
            lastClickTime = -1f;
        }
        else
        {
            // First click of a possible pair (or a click on empty space / a
            // different object). Just record it and don't toggle the outline yet.
            lastClickedOutline = hitOutline;
            lastClickTime = Time.time;

            if (hitOutline == null)
            {
                Deselect();
            }
        }
    }

    private void Select(Outline outline)
    {
        if (currentlySelected == outline) return;

        Deselect();
        currentlySelected = outline;
        currentlySelected.enabled = true;
    }

    private void Deselect()
    {
        if (currentlySelected != null)
        {
            currentlySelected.enabled = false;
            currentlySelected = null;
        }
    }
}