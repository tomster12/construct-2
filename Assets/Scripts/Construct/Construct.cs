using System;
using System.Collections.Generic;
using UnityEngine;


public class Construct : MonoBehaviour
{
    private ConstructPart corePart;
    private ConstructMovement controlledMovement;
    private HashSet<ConstructPart> constructParts = new HashSet<ConstructPart>();
    private HashSet<ConstructMovement> constructMovements = new HashSet<ConstructMovement>();
    public Dictionary<string, ConstructAction> constructActions = new Dictionary<string, ConstructAction>();

    public void Move(Vector3 dir)
    {
        if (controlledMovement != null) controlledMovement.Move(dir);
    }

    public void Aim(Vector3 pos)
    {
        if (controlledMovement != null) controlledMovement.Aim(pos);
    }

    public ConstructAction UseActionDown(string actionName)
    {
        if (constructActions.ContainsKey(actionName))
        {
            constructActions[actionName].UseDown();
            return constructActions[actionName];
        }
        return null;
    }

    public ConstructAction UseActionUp(string actionName)
    {
        if (constructActions.ContainsKey(actionName))
        {
            constructActions[actionName].UseUp();
            return constructActions[actionName];
        }
        return null;
    }

    public void PickBestMovement()
    {
        if (controlledMovement != null) return;
        foreach (ConstructMovement movement in constructMovements)
        {
            if (movement.CanSetControlled(true))
            {
                SetControlledMovement(movement);
                return;
            }
        }
    }

    public void AddPart(ConstructPart part)
    {
        if (part.IsConstructed) throw new Exception("Cannot AddPart(part) when part already constructed.");
        constructParts.Add(part);
        part.OnJoinConstruct(this);
    }

    public void RemovePart(ConstructPart part)
    {
        if (!part.IsConstructed) throw new Exception("Cannot RemovePart(part) when part not constructed.");
        if (!constructParts.Contains(part)) throw new Exception("Cannot RemovePart(part), not registered!");
        constructParts.Remove(part);
        part.OnleaveConstruct(this);
    }

    public void RegisterPartMovement(ConstructMovement movement)
    {
        if (constructMovements.Contains(movement)) throw new Exception("Cannot RegisterPartMovement(movement), already registered!");
        constructMovements.Add(movement);
        PickBestMovement();
    }

    public void DeregisterPartMovement(ConstructMovement movement)
    {
        if (!constructMovements.Contains(movement)) throw new Exception("Cannot DeregisterPartMovement(movement), not registered!");
        if (controlledMovement == movement) SetControlledMovement(null);
        constructMovements.Remove(movement);
        PickBestMovement();
    }

    public void RegisterAction(ConstructAction action)
    {
        if (constructActions.ContainsKey(action.ActionName)) throw new Exception("Cannot RegisterAction(action), already registered!");
        constructActions.Add(action.ActionName, action);
    }

    public void DeregisterAction(ConstructAction action)
    {
        if (!constructActions.ContainsKey(action.ActionName)) throw new Exception("Cannot DeregisterAction(action), not registered!");
        constructActions.Remove(action.ActionName);
    }

    public void SetAndAddCore(ConstructPart corePart)
    {
        if (this.corePart != null) throw new Exception("Cannot SetCore() when already have a core.");
        this.corePart = corePart;
        AddPart(this.corePart);
    }

    private void SetControlledMovement(ConstructMovement movement)
    {
        if (movement != null && movement.isControlled) throw new Exception("Cannot SetControlledMovement(movement) on a movement that already isControlled.");
        if (controlledMovement != null) controlledMovement.SetControlled(false);
        controlledMovement = movement;
        if (controlledMovement != null) controlledMovement.SetControlled(true);
    }
}
