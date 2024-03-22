using System;
using System.Collections.Generic;
using UnityEngine;

public class Construct : MonoBehaviour
{
    public void Move(Vector3 dir) => controllingMovement?.Move(dir);

    public void Aim(Vector3 pos) => controllingMovement?.Aim(pos);

    public void ActionInputDown(string actionName) => actionSet.ActionInputDown(actionName);

    public void ActionInputUp(string actionName) => actionSet.ActionInputUp(actionName);

    public void UpdateControllingMovement()
    {
        if (controllingMovement != null) return;
        foreach (IConstructMovement movement in movements)
        {
            if (movement.CanSetControlling())
            {
                SetControllingMovement(movement);
                return;
            }
        }
    }

    public void AddPart(ConstructPart part)
    {
        if (part.IsConstructed) throw new Exception("Cannot AddPart(part) when part already constructed.");
        parts.Add(part);
        part.OnJoinConstruct(this);
    }

    public void RemovePart(ConstructPart part)
    {
        if (!part.IsConstructed) throw new Exception("Cannot RemovePart(part) when part not constructed.");
        if (!parts.Contains(part)) throw new Exception("Cannot RemovePart(part), not registered!");
        parts.Remove(part);
        part.OnleaveConstruct(this);
    }

    public void RegisterPartMovement(IConstructMovement movement)
    {
        if (movements.Contains(movement)) throw new Exception("Cannot RegisterPartMovement(movement), already registered!");
        movements.Add(movement);
    }

    public void DeregisterPartMovement(IConstructMovement movement)
    {
        if (!movements.Contains(movement)) throw new Exception("Cannot DeregisterPartMovement(movement), not registered!");
        if (controllingMovement == movement) UnsetControllingMovement();
        movements.Remove(movement);
    }

    public bool RegisterAction(ConstructAction action) => actionSet.RegisterAction(action);

    public bool DeregisterAction(ConstructAction action) => actionSet.DeregisterAction(action);

    public void InitCore(ConstructPart corePart)
    {
        if (this.corePart != null) throw new Exception("Cannot SetCore() when already have a core.");
        this.corePart = corePart;
        AddPart(this.corePart);
        UpdateControllingMovement();
    }

    private HashSet<ConstructPart> parts = new HashSet<ConstructPart>();
    private HashSet<IConstructMovement> movements = new HashSet<IConstructMovement>();
    private ConstructActionSet actionSet;
    private IConstructMovement controllingMovement;
    private ConstructPart corePart;

    private void Awake()
    {
        actionSet = new ConstructActionSet(this);
    }

    private void SetControllingMovement(IConstructMovement movement)
    {
        if (movement == null) throw new Exception("Cannot SetControllingMovement(null).");
        if (!movements.Contains(movement)) throw new Exception("Cannot SetControllingMovement(movement) when movement not registered.");
        if (movement.IsControlling) throw new Exception("Cannot SetControllingMovement(movement) when movement already controlling.");
        if (controllingMovement != null) UnsetControllingMovement();
        controllingMovement.SetControlling();
        controllingMovement = movement;
    }

    private void UnsetControllingMovement()
    {
        if (controllingMovement == null) Utility.LogWarning("Cannot UnsetControllingMovement() when no controlled movement.");
        controllingMovement.UnsetControlling();
        controllingMovement = null;
    }
}
