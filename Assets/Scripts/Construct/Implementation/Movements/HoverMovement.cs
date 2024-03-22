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

    public bool CanAttach(AttacheeComponent attachee) => CanSetControlling();

    public bool CanDetach() => CanUnsetControlling();

    public void SetControlling()
    {
        if (!CanSetControlling()) throw new System.Exception("Cannot SetControlling(true) when already controlled.");
        IsControlling = true;
        part.SetController(this);
    }

    public void UnsetControlling()
    {
        if (!CanUnsetControlling()) throw new System.Exception("Cannot UnsetControlling(false) when not controlled.");
        IsControlling = false;
        part.UnsetController();
    }

    public bool CanSetControlling() => CanTransition;

    public bool CanUnsetControlling() => CanTransition;

    [SerializeField] private ConstructPart part;
}
