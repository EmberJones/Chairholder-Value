using UnityEngine;

public enum StampType { Approve, Reject }

public class StampObject : DraggableObject
{
    [Header("Stamp Identity")]
    [SerializeField] private StampType stampType;

    public StampType StampType => stampType;

    protected override void OnPickedUp()
    {
        base.OnPickedUp();
    }

    protected override void OnDropped()
    {
        base.OnDropped();   
    }
}