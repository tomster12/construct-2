using System.Collections;
using UnityEngine;

public class HoverMovement : ConstructMovement, IAttacherMovement
{
    [SerializeField] private ConstructPart part;

    public override bool IsBlocking => IsAttachTransitioning;
    public bool IsAttachTransitioning { get; private set; }

    public override void Aim(Vector3 pos)
    {
        // TODO: Implement aim
    }

    public override void Move(Vector3 dir)
    {
        // TODO: Implement move
    }

    public IEnumerator Attach(AttacheeComponent atachee)
    {
        // TODO: Implement attach
        IsAttachTransitioning = true;
        yield return new WaitForSeconds(1f);
        IsAttachTransitioning = false;
    }

    public IEnumerator Detach()
    {
        // TODO: Implement detach
        IsAttachTransitioning = true;
        yield return new WaitForSeconds(1f);
        IsAttachTransitioning = false;
    }

    public bool CanAttach(AttacheeComponent attachee) => CanSetControlled(true);

    public bool CanDetach() => CanSetControlled(false);

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
