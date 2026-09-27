using UnityEngine;

public class FaxMachine : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip faxSound;
    [SerializeField] private AudioClip rejectSound;

    private AudioSource _audio;
    private IRoundManager _roundManager;

    void Awake() => _audio = GetComponent<AudioSource>();

    void Start()
    {
        _roundManager = GameManager.Instance as IRoundManager;
        if (_roundManager == null)
            _roundManager = TutorialManager.Instance as IRoundManager;

        if (_roundManager == null)
            Debug.LogError("[FaxMachine] No GameManager or TutorialManager found in this scene.");
    }

    public void TrySubmitRound()
    {
        if (_roundManager == null) return;

        if (!_roundManager.CanSubmitRound)
        {
            if (_audio != null && rejectSound != null) _audio.PlayOneShot(rejectSound);
            return;
        }

        if (animator != null) animator.SetTrigger("Print");
        if (_audio != null && faxSound != null) _audio.PlayOneShot(faxSound);

        _roundManager.SubmitRound();
    }
}