using UnityEngine;

/// <summary>
/// Attach this to any object with a Renderer to give it an outline.
/// Toggle it on/off via the "enabled" property (e.g. outline.enabled = true).
/// Uses the inverted-hull technique: an extra material is appended to the
/// renderer, which makes Unity redraw the same mesh with the Outline shader
/// (see Outline.shader), expanded outward and culled to only show its silhouette.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class Outline : MonoBehaviour
{
    [SerializeField] private Color outlineColor = new Color(1f, 0.8f, 0f, 1f);
    [SerializeField] private float outlineWidth = 0.015f;

    [Header("Flat Objects (planes/quads)")]
    [Tooltip("Enable for flat planes/quads, where every normal points the same " +
        "way so normal-based extrusion does nothing. Extrudes radially across " +
        "the surface instead.")]
    [SerializeField] private bool isFlatObject = false;
    [Tooltip("Object-space offset of the mesh's visual center, only used when " +
        "Is Flat Object is on. Leave at (0,0,0) if the pivot is already centered " +
        "on the plane.")]
    [SerializeField] private Vector3 flatModePivotOffset = Vector3.zero;

    [Tooltip("Pushes the outline backward along the normal to avoid z-fighting " +
        "with the original surface. Increase this if you still see flickering; " +
        "decrease it if the outline starts to look detached from the object.")]
    [SerializeField] private float depthBias = 0.002f;

    private static Shader outlineShader;
    private Renderer objectRenderer;
    private Material outlineMaterialInstance;
    private Material[] originalMaterials;
    private bool outlineMaterialAdded;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();

        if (outlineShader == null)
        {
            outlineShader = Shader.Find("Custom/Outline");
            if (outlineShader == null)
            {
                Debug.LogError("Outline: Could not find shader 'Custom/Outline'. " +
                    "Make sure Outline.shader is in the project.");
            }
        }

        // Always start off, regardless of the Enabled checkbox left on the
        // component/prefab. Prevents outlines showing up the instant an
        // object spawns; SelectionManager turns this on via double-click.
        enabled = false;
    }

    private void OnEnable()
    {
        if (outlineShader == null) return;

        if (outlineMaterialInstance == null)
        {
            outlineMaterialInstance = new Material(outlineShader);
        }
        outlineMaterialInstance.SetColor("_OutlineColor", outlineColor);
        outlineMaterialInstance.SetFloat("_OutlineWidth", outlineWidth);
        outlineMaterialInstance.SetFloat("_FlatMode", isFlatObject ? 1f : 0f);
        outlineMaterialInstance.SetVector("_PivotOffset", flatModePivotOffset);
        outlineMaterialInstance.SetFloat("_DepthBias", depthBias);

        AddOutlineMaterial();
    }

    private void OnDisable()
    {
        RemoveOutlineMaterial();
    }

    private void OnDestroy()
    {
        if (outlineMaterialInstance != null)
        {
            Destroy(outlineMaterialInstance);
        }
    }

    private void AddOutlineMaterial()
    {
        if (outlineMaterialAdded) return;

        originalMaterials = objectRenderer.sharedMaterials;
        Material[] newMaterials = new Material[originalMaterials.Length + 1];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = originalMaterials[i];
        }
        newMaterials[newMaterials.Length - 1] = outlineMaterialInstance;

        objectRenderer.materials = newMaterials;
        outlineMaterialAdded = true;
    }

    private void RemoveOutlineMaterial()
    {
        if (!outlineMaterialAdded) return;
        if (objectRenderer != null && originalMaterials != null)
        {
            objectRenderer.materials = originalMaterials;
        }
        outlineMaterialAdded = false;
    }

    /// <summary>Change the outline color at runtime.</summary>
    public void SetColor(Color color)
    {
        outlineColor = color;
        if (outlineMaterialInstance != null)
        {
            outlineMaterialInstance.SetColor("_OutlineColor", outlineColor);
        }
    }

    /// <summary>Change the outline width at runtime.</summary>
    public void SetWidth(float width)
    {
        outlineWidth = width;
        if (outlineMaterialInstance != null)
        {
            outlineMaterialInstance.SetFloat("_OutlineWidth", outlineWidth);
        }
    }
}