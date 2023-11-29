using System;
using UnityEngine;

public class ConstructPart : MonoBehaviour
{
    [SerializeField] private ConstructMovement inherentMovement;

    private Construct currentConstruct;
    private IPartController currentController;

    public Action<Construct> OnJoinConstructEvent = delegate { };
    public Action OnLeaveConstructEvent = delegate { };
    public bool IsConstructed => currentConstruct != null;
    public bool IsControlled => currentController != null;
    public IPartController CurrentController => currentController;
    public Construct Construct => currentConstruct;

    public void OnJoinConstruct(Construct construct)
    {
        if (currentConstruct != null) throw new Exception("Cannot OnJoinConstruct(construct) when already joined to construct.");
        currentConstruct = construct;
        if (inherentMovement != null) currentConstruct.RegisterPartMovement(inherentMovement);
        OnJoinConstructEvent(construct);
    }

    public void OnleaveConstruct(Construct construct)
    {
        if (currentConstruct != construct) throw new Exception("Cannot OnleaveConstruct(construct) when not joined to construct.");
        if (inherentMovement != null) currentConstruct.DeregisterPartMovement(inherentMovement);
        OnLeaveConstructEvent();
        currentConstruct = null;
    }

    public T GetPartComponent<T>() where T : PartComponent => GetComponent<T>();

    public void SetController(IPartController controller)
    {
        if (currentController != null) throw new Exception("Cannot SetController(controller) when already have a controller.");
        currentController = controller;
    }
}
