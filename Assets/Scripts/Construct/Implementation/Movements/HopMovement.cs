using UnityEngine;
using UnityEngine.Assertions;

public class HopMovement : ConstructMovement
{
    public bool IsGrounded { get; private set; }

    public override void Aim(Vector3 pos) => Debug.LogWarning("HopMovement.Aim(pos) not implemented.");

    public override void Move(Vector3 dir) => Debug.LogWarning("HopMovement.Move(dir) not implemented.");

    public override bool CanActivate() => !IsActive && part.CanControl;

    public override void Activate()
    {
        Assert.IsTrue(CanActivate());
        partPC = part.TakeControl(this);
        IsActive = true;
        OnStateChange.Invoke(IsActive);
    }

    public override void Deactivate()
    {
        Assert.IsTrue(IsActive);
        partPC.Release();
        IsActive = false;
        OnStateChange.Invoke(IsActive);
    }

    public override Vector3 GetCentre() => part.GetCentre();

    [Header("References")]
    [SerializeField] private ConstructPart part;

    private ConstructPart.PhysicalHandle partPC;

    private void Awake()
    {
        Assert.IsTrue(part != null);
    }
}
