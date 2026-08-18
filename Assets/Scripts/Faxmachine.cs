using UnityEngine;


public class FaxMachine : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer readyIndicator;   
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color notReadyColor = Color.red;

    [Header("Animation")]
    [SerializeField] private Animator animator;                
    [SerializeField] private AudioClip faxSound;

    private bool _ready = false;
    private AudioSource _audio;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        SetReady(false);
    }

    void OnMouseDown()
    {
        if (!_ready) return;
        Submit();
    }

    public void SetReady(bool ready)
    {
        _ready = ready;

        if (readyIndicator != null)
            readyIndicator.color = ready ? readyColor : notReadyColor;
    }

    void Submit()
    {
        if (animator != null)
            animator.SetTrigger("Print");

        if (_audio != null && faxSound != null)
            _audio.PlayOneShot(faxSound);

        SetReady(false);
        GameManager.Instance?.SubmitCurrentCV();
    }
}