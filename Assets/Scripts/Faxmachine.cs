using UnityEngine;

public class FaxMachine : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip faxSound;
    [SerializeField] private AudioClip rejectSound;   // plays if nothing's approved yet

    private AudioSource _audio;
    void Awake() => _audio = GetComponent<AudioSource>();

    public void TrySubmitRound()
    {
        if (!GameManager.Instance.CanSubmitRound)
        {
            if (_audio != null && rejectSound != null) _audio.PlayOneShot(rejectSound);
            return;
        }

        if (animator != null) animator.SetTrigger("Print");
        if (_audio != null && faxSound != null) _audio.PlayOneShot(faxSound);

        GameManager.Instance.SubmitRound();
    }
}