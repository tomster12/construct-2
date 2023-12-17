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

    public bool CanAttach(AttacheeComponent attachee) => CanSetControlling();

    public bool CanDetach() => CanUnsetControlling();

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
