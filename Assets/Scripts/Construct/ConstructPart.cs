using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldObject))]
public class ConstructPart : MonoBehaviour
{
    public Action<Construct> OnJoinConstructEvent = delegate { };
    public Action OnLeaveConstructEvent = delegate { };
    public WorldObject WO { get; private set; }
    public ConstructMovement Movement => inherentMovement;
    public IPartController CurrentController => currentController;
    public Construct Construct => currentConstruct;
    public bool IsConstructed => currentConstruct != null;
    public bool IsControlled => currentController != null;

    public T GetPartComponent<T>() where T : PartComponent
    {
        Type type = typeof(T);
        if (partComponents.ContainsKey(type))
        {
            return partComponents[type] as T;
        }
        return null;
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

    public void SetController(IPartController controller)
    {
        if (controller == null) throw new Exception("Cannot SetController(null).");
        if (currentController != null) throw new Exception("Cannot SetController(controller) when already have a controller.");
        currentController = controller;
    }

    public void UnsetController()
    {
        if (currentController == null) throw new Exception("Cannot UnsetController() when not controlled.");
        currentController = null;
    }

    [SerializeField] private ConstructMovement inherentMovement;
    private Dictionary<Type, Component> partComponents = new Dictionary<Type, Component>();
    private Construct currentConstruct;
    private IPartController currentController;

    private void Awake()
    {
        WO = GetComponent<WorldObject>();

        // Cache all part components
        foreach (PartComponent component in GetComponents<PartComponent>())
        {
            partComponents[component.GetType()] = component;
        }
    }
}
