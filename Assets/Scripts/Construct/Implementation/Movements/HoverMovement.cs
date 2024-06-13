using System.Collections;
using UnityEngine;

public class HoverMovement : MonoBehaviour, IAttacherMovement
{
    public bool IsControlling { get; private set; }
    public bool CanTransition => !IsAttachTransitioning;
    public bool IsAttachTransitioning { get; private set; }

    public void Aim(Vector3 pos)
    {
        // TODO: Implement
    }

    public void Move(Vector3 dir)
    {
        // TODO: Implement
    }

    public IEnumerator Attach(AttacheeComponent atachee)
    {
        // TODO: Implement
        IsAttachTransitioning = true;
        yield return new WaitForSeconds(1f);
        IsAttachTransitioning = false;
    }

    public IEnumerator Detach()
    {
        // TODO: Implement
        IsAttachTransitioning = true;
        yield return new WaitForSeconds(1f);
        IsAttachTransitioning = false;
    }

    public bool CanAttach(AttacheeComponent attachee) => IsControlling && !attachee.Part.IsConstructed && CanUnsetControlling();

    public bool CanDetach() => !IsControlling && CanSetControlling();

    public void SetControlling()
    {
        if (!CanSetControlling()) throw new System.Exception("SetControlling(): !CanSetControlling().");
        IsControlling = true;
        part.SetController(this);
    }

    public void UnsetControlling()
    {
        if (!CanUnsetControlling()) throw new System.Exception("UnsetControlling(): !CanUnsetControlling.");
        IsControlling = false;
        part.UnsetController();
    }

    public bool CanSetControlling() => CanTransition;

    public bool CanUnsetControlling() => CanTransition;

    public bool CanEnterForging() => false;

    public bool CanExitForging() => true;

    [SerializeField] private ConstructPart part;
}
