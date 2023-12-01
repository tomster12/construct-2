using System.Collections;
using UnityEngine;

public class HoverMovement : ConstructMovement, IAttacherMovement
{
    [SerializeField] private ConstructPart part;

    public override bool IsBlocking => IsAttachTransitioning;
    public bool IsAttachTransitioning { get; private set; }

    public override void Aim(Vector3 pos)
    {
        // TODO: Aim
    }

    public override void Move(Vector3 dir)
    {
        // TODO: Move
    }

    public IEnumerator Attach(AttacheeComponent atachee)
    {
        // TODO: Attach
        IsAttachTransitioning = true;
        yield return new WaitForSeconds(1f);
        IsAttachTransitioning = false;
    }

    public IEnumerator Detach()
    {
        // TODO: Detach
        IsAttachTransitioning = true;
        yield return new WaitForSeconds(1f);
        IsAttachTransitioning = false;
    }

    public bool CanAttach(AttacheeComponent attachee) => CanSetControlling(true);

    public bool CanDetach() => CanSetControlling(false);

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
