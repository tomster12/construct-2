using System;
using System.Collections.Generic;
using UnityEngine;


public class Construct : MonoBehaviour
{
    private ConstructPart corePart;
    private ConstructMovement currentMovement;
    private HashSet<ConstructPart> constructParts = new HashSet<ConstructPart>();
    private HashSet<ConstructMovement> constructMovements = new HashSet<ConstructMovement>();
    public Dictionary<string, ConstructAction> constructActions = new Dictionary<string, ConstructAction>();

    public void Move(Vector3 dir) { if (currentMovement != null) Move(dir); }

    public void Aim(Vector3 pos) { if (currentMovement != null) Aim(pos); }

    public ConstructAction UseDownAction(string actionName)
    {
        if (constructActions.ContainsKey(actionName))
        {
            constructActions[actionName].UseDown();
            return constructActions[actionName];
        }
        return null;
    }

    public ConstructAction UseUpAction(string actionName)
    {
        if (constructActions.ContainsKey(actionName))
        {
            constructActions[actionName].UseUp();
            return constructActions[actionName];
        }
        return null;
    }

    public void VisualiseAllActions() { foreach (ConstructAction action in constructActions.Values) action.Visualise(); }

    public void AddPart(ConstructPart part)
    {
        if (part.IsConstructed) throw new Exception("Cannot AddPart(part) when part already constructed.");
        if (constructParts.Contains(part)) throw new Exception("Cannot AddPart(part), already registered!");
        constructParts.Add(part);
        part.OnJoinConstruct(this);
    }

    public void RemovePart(ConstructPart part)
    {
        if (!constructParts.Contains(part)) throw new Exception("Cannot RemovePart(part), not registered!");
        if (!part.IsConstructed) throw new Exception("Cannot RemovePart(part) when part not constructed.");
        constructParts.Remove(part);
        part.OnleaveConstruct(this);
    }

    public void RegisterPartMovement(ConstructMovement movement)
    {

        if (constructMovements.Contains(movement)) throw new Exception("Cannot RegisterPartMovement(movement), already registered!");
        constructMovements.Add(movement);
        if (currentMovement == null) SetMovement(movement);
    }

    public void DeregisterPartMovement(ConstructMovement movement)
    {
        if (!constructMovements.Contains(movement)) throw new Exception("Cannot DeregisterPartMovement(movement), not registered!");
        if (currentMovement == movement) SetMovement(null);
        constructMovements.Remove(movement);
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

    public void SetCore(ConstructPart corePart)
    {
        if (this.corePart != null) throw new Exception("Cannot SetCore() when already have a core.");
        this.corePart = corePart;
        AddPart(this.corePart);
    }

    public void SetMovement(ConstructMovement movement)
    {
        if (movement != null && movement.IsControlling) throw new Exception("Cannot SetMovement(movement) on a movement that already IsControlling.");
        if (currentMovement != null) currentMovement.SetControlling(false);
        currentMovement = movement;
        if (currentMovement != null) currentMovement.SetControlling(true);
    }
}
