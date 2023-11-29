using System.Collections;
using UnityEngine;

public class HoverMovement : ConstructMovement, IAttacherMovement
{
    [SerializeField] ConstructPart part;

    public override bool IsControlling { get; protected set; }
    public override bool IsBlocking => IsAttachTransitioning;
    public bool IsAttachTransitioning { get; private set; }

    private void Update()
    {

    }

    public override void Aim(Vector3 pos)
    {
        Debug.Log("HoverMovement.Aim(pos) not implemented.");
    }

    public override void Move(Vector3 dir)
    {
        Debug.Log("HoverMovement.Move(dir) not implemented.");
    }

    public IEnumerator Attach(AttacheePartComponent atachee)
    {
        Debug.Log("HoverMovement.Attach(atachee) not implemented.");
        IsAttachTransitioning = true;
        yield return new WaitForSeconds(1f);
        IsAttachTransitioning = false;
    }

    public IEnumerator Detach()
    {
        Debug.Log("HoverMovement.Detach() not implemented.");
        IsAttachTransitioning = true;
        yield return new WaitForSeconds(1f);
        IsAttachTransitioning = false;
    }

    public bool CanAttach(AttacheePartComponent attachee)
    {
        return IsControlling && !IsBlocking;
    }

    public bool CanDetach()
    {
        return !IsControlling && !IsBlocking;
    }

    public override void SetControlling(bool isControlling)
    {
        IsControlling = true;
        if (isControlling) part.SetController(this);
        else part.SetController(null);
    }
}
