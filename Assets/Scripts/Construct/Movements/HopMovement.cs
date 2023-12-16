using System.Collections;
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

    public override void SetControlled(bool isControlled)
    {
        if (!CanSetControlled(isControlled)) throw new System.Exception("Cannot SetControlling(true) when CanControl is false.");
        isControlled = true;
        if (isControlled) part.SetController(this);
        else part.SetController(null);
    }

    public override bool CanSetControlled(bool isControlled)
    {
        return !IsBlocking;
    }
}
