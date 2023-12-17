using UnityEngine;

public class HopMovement : ConstructMovement
{
    [SerializeField] private ConstructPart part;

    public override bool IsBlocking => !IsGrounded;
    public bool IsGrounded { get; private set; }

    public override void Aim(Vector3 pos)
    {
        Debug.Log("HopMovement.Aim(pos) not implemented.");
    }

    public override void Move(Vector3 dir)
    {
        Debug.Log("HopMovement.Move(dir) not implemented.");
    }

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

    public override bool CanSetControlling()
    {
        return !IsBlocking;
    }

    public override bool CanUnsetControlling()
    {
        return !IsBlocking;
    }
}
