using System;
using UnityEngine;

[RequireComponent(typeof(WorldObject))]
public class ConstructPart : MonoBehaviour
{
    [SerializeField] private ConstructMovement inherentMovement;
    private Construct currentConstruct;
    private IPartController currentController;

    public Action<Construct> OnJoinConstructEvent = delegate { };
    public Action OnLeaveConstructEvent = delegate { };
    public Action OnPartChangeEvent = delegate { };

    public WorldObject WO { get; private set; }
    public bool IsConstructed => currentConstruct != null;
    public bool IsControlled => currentController != null;
    public ConstructMovement Movement => inherentMovement;
    public IPartController CurrentController => currentController;
    public Construct Construct => currentConstruct;

    private void Awake()
    {
        WO = GetComponent<WorldObject>();

    }

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
        if (controller != null && currentController != null) throw new Exception("Cannot SetController(controller) when already have a controller.");
        currentController = controller;
    }
}
