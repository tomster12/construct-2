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

    public override void SetControlling(bool isControlling)
    {
        if (!CanSetControlling(isControlling)) throw new System.Exception("Cannot SetControlling(true) when CanControl is false.");
        IsControlling = true;
        if (isControlling) part.SetController(this);
        else part.SetController(null);
    }

    public override bool CanSetControlling(bool isControlling)
    {
        return !IsBlocking;
    }
}
