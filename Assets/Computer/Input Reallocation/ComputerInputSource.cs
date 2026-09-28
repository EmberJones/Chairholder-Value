using UnityEngine;
using UnityEngine.Events;
public class ComputerInputSource : MonoBehaviour
{
    public static ComputerInputSource Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] LayerMask RayCastMask = ~0;        // filtering down to only hit the screen, we don't need to raycast to the desk for eg.
    [SerializeField] float RayCastDistance = 500; // should probably be shorter when used in FirstPersonScene
    public bool Active = true;

    [Header("Events")]
    [SerializeField] UnityEvent<Vector2> OnCursorInput = new UnityEvent<Vector2>();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!Active) return;

        Ray MouseRay = Camera.main.ScreenPointToRay( Input.mousePosition );     // get a ray based on the mouse location

        RaycastHit HitResult;

        if (Physics.Raycast(MouseRay, out HitResult, RayCastDistance, RayCastMask, QueryTriggerInteraction.Ignore))     // raycast to see what we have hit
        {
            if (HitResult.collider.gameObject != gameObject) return;        // we only want to raycast hit this screen/textured plane

            OnCursorInput.Invoke(HitResult.textureCoord);       // send the "input" to the canvas
        }
    }
}


// guide to developing this from: https://www.youtube.com/watch?v=fXsdK2umVmM&t=6s