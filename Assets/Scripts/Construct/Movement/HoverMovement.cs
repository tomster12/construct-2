using UnityEngine;

public class HoverMovement : ConstructMovement
{
    [SerializeField] ConstructPart part;

    public override bool IsControlling { get; protected set; }

    public override void Aim(Vector3 pos)
    {
        Debug.Log("HoverMovement.Aim(pos) not implemented.");
    }

    public override void Move(Vector3 dir)
    {
        Debug.Log("HoverMovement.Move(dir) not implemented.");
    }

    public override void SetControlling(bool isControlling)
    {
        IsControlling = true;
        if (isControlling) part.SetController(this);
        else part.SetController(null);
    }
}
