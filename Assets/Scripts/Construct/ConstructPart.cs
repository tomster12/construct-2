using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldObject))]
public class ConstructPart : MonoBehaviour
{
    public WorldObject WO { get; private set; }
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
        foreach (IConstructMovement movement in movements) construct.RegisterMovement(movement);
        foreach (Action skill in skills) construct.RegisterSkill(skill);
    }

    public void OnleaveConstruct(Construct construct)
    {
        if (currentConstruct != construct) throw new Exception("Cannot OnleaveConstruct(construct) when not joined to construct.");
        foreach (IConstructMovement movement in movements) construct.RegisterMovement(movement);
        foreach (Action skill in skills) construct.RegisterSkill(skill);
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

    public void RegisterMovement(IConstructMovement movement)
    {
        if (movements.Contains(movement)) throw new Exception("Cannot RegisterMovement(movement), already registered!");
        movements.Add(movement);
        if (IsConstructed) currentConstruct.RegisterMovement(movement);
    }

    public void UnregisterMovement(IConstructMovement movement)
    {
        if (!movements.Contains(movement)) throw new Exception("Cannot UnregisterMovement(movement), not registered!");
        movements.Remove(movement);
        if (IsConstructed) currentConstruct.UnregisterMovement(movement);
    }

    public void RegisterSkill(Action skill)
    {
        if (skills.Contains(skill)) throw new Exception("Cannot RegisterSkill(skill), already registered!");
        skills.Add(skill);
        if (IsConstructed) currentConstruct.RegisterSkill(skill);
    }

    public void UnregisterSkill(Action skill)
    {
        if (!skills.Contains(skill)) throw new Exception("Cannot UnregisterSkill(skill), not registered!");
        skills.Remove(skill);
        if (IsConstructed) currentConstruct.UnregisterSkill(skill);
    }

    private Dictionary<Type, Component> partComponents = new Dictionary<Type, Component>();
    private HashSet<IConstructMovement> movements = new HashSet<IConstructMovement>();
    private HashSet<Action> skills = new HashSet<Action>();
    private Construct currentConstruct;
    private IPartController currentController;

    private void Awake()
    {
        WO = GetComponent<WorldObject>();

        // Cache and init all part components
        foreach (PartComponent component in GetComponents<PartComponent>())
        {
            partComponents[component.GetType()] = component;
            component.Init(this);
        }

        // Register all found movements and skills
        foreach (IConstructMovement movement in GetComponents<IConstructMovement>()) RegisterMovement(movement);
        foreach (Action skill in GetComponents<Action>()) RegisterSkill(skill);
    }
}
