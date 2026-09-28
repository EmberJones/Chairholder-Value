using UnityEngine;

public enum StampType { Approve, Reject }

public class StampObject : DraggableObject
{
    [Header("Stamp Identity")]
    [SerializeField] private StampType stampType;

    [Header("Animation")]
    [SerializeField] private Animator animator;  
    [SerializeField] private string stampTriggerName = "Stamp";

    [Header("Upright Correction")]
    [SerializeField] private Vector3 uprightEuler = Vector3.zero; 
    public Quaternion UprightRotation => Quaternion.Euler(uprightEuler);
    public StampType StampType => stampType;

    protected override void OnPickedUp() => base.OnPickedUp();
    protected override void OnDropped() => base.OnDropped();

    public void PlayStampAnimation()
    {
        if (animator != null)
            animator.SetTrigger(stampTriggerName);
    }

}