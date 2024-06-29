using UnityEngine;

public class HoverMovement : ConstructMovement, IAttacherMovement
{
    public override bool IsControlling { get; protected set; }

    // TODO: Implement
    public override void Aim(Vector3 pos) => Debug.LogWarning("HopMovement.Aim(pos) not implemented.");

    // TODO: Implement
    public override void Move(Vector3 dir) => Debug.LogWarning("HopMovement.Move(dir) not implemented.");

    public override void SetControlling()
    {
        if (!CanSetControlling()) throw new System.Exception("SetControlling(): !CanSetControlling().");
        IsControlling = true;
        part.SetController(this);
    }

    public override void UnsetControlling()
    {
        if (!CanUnsetControlling()) throw new System.Exception("UnsetControlling(): !CanUnsetControlling.");
        IsControlling = false;
        part.UnsetController();
    }

    public override bool CanSetControlling() => canTransition;

    public override bool CanUnsetControlling() => canTransition;

    public override bool CanEnterForging() => false;

    public override bool CanExitForging() => true;

    public override Vector3 GetCentre() => part.GetCentre();

    [Header("References")]
    [SerializeField] private ConstructPart part;

    private bool canTransition => !isAttachTransitioning;
    private bool isAttachTransitioning;
}
