using UnityEngine;

public class HopMovement : ConstructMovement
{
    public override bool IsControlling { get; protected set; }
    public bool IsGrounded { get; private set; }

    // TODO: Implement
    public override void Aim(Vector3 pos) => Debug.LogWarning("HopMovement.Aim(pos) not implemented.");

    // TODO: Implement
    public override void Move(Vector3 dir) => Debug.LogWarning("HopMovement.Move(dir) not implemented.");

    public override void SetControlling()
    {
        if (!CanSetControlling()) throw new System.Exception("Cannot SetControlling(true) when already controlled.");
        IsControlling = true;
        part.SetController(this);
    }

    public override void UnsetControlling()
    {
        if (!CanUnsetControlling()) throw new System.Exception("Cannot UnsetControlling(false) when not controlled.");
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

    private bool canTransition => IsGrounded;
}
