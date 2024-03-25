using System;
using System.Collections.Generic;
using UnityEngine;

public class Construct : MonoBehaviour
{
    public void Move(Vector3 dir) => controllingMovement?.Move(dir);

    public void Aim(Vector3 pos) => controllingMovement?.Aim(pos);

    public void SkillInputDown(int slot) => assignedSkills.ActionInputDown(slot);

    public void SkillInputUp(int slot) => assignedSkills.ActionInputUp(slot);

    public void ConstructionInputDown() => constructions[0].InputDown();

    public void ConstructionInputUp() => constructions[0].InputUp();

    public void EnterForging()
    {
        // Check all part controllers
        bool canEnter = true;
    }

    public void ExitForging()
    {
    }

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

    public void RegisterMovement(IConstructMovement movement)
    {
        if (movements.Contains(movement)) throw new Exception("Cannot RegisterPartMovement(movement), already registered!");
        movements.Add(movement);
    }

    public void UnregisterMovement(IConstructMovement movement)
    {
        if (!movements.Contains(movement)) throw new Exception("Cannot UnregisterPartMovement(movement), not registered!");
        if (controllingMovement == movement) UnsetControllingMovement();
        movements.Remove(movement);
    }

    public void RegisterSkill(Action action, int slot = -1)
    {
        if (skills.Contains(action)) throw new Exception("RegisterSkill(action): already registered.");
        skills.Add(action);
        if (assignedSkills.AvailableSlotCount > 0) assignedSkills.RegisterAction(action, slot);
    }

    public void UnregisterSkill(Action action)
    {
        if (!skills.Contains(action)) throw new Exception("UnregisterSkill(action): not registered.");
        if (action.IsAssigned) assignedSkills.UnregisterAction(action);
        skills.Remove(action);
    }

    public void RegisterConstruction(Action action)
    {
        if (constructions.Contains(action)) throw new Exception("RegisterConstructionAction(action): already registered.");
        constructions.Add(action);
    }

    public void UnregisterConstruction(Action action)
    {
        if (!constructions.Contains(action)) throw new Exception("UnregisterConstructionAction(action): not registered.");
        constructions.Remove(action);
    }

    public void RegisterShape(IConstructShape shape)
    {
        if (shapes.Contains(shape)) throw new Exception("RegisterShape(shape): Already registered.");
        shapes.Add(shape);
    }

    public void UnregisterShape(IConstructShape shape)
    {
        if (shapes.Contains(shape)) throw new Exception("UnregisterShape(shape): not registered.");
        shapes.Remove(shape);
    }

    public void InitCore(ConstructPart corePart)
    {
        if (this.corePart != null) throw new Exception("Cannot SetCore() when already have a core.");
        this.corePart = corePart;
        AddPart(this.corePart);
        UpdateControllingMovement();
    }

    private HashSet<ConstructPart> parts = new HashSet<ConstructPart>();
    private HashSet<IConstructMovement> movements = new HashSet<IConstructMovement>();
    private HashSet<IConstructShape> shapes = new HashSet<IConstructShape>();
    private HashSet<Action> skills = new HashSet<Action>();
    private List<Action> constructions = new List<Action>();
    private ActionSet assignedSkills = new ActionSet();
    private IConstructMovement controllingMovement;
    private ConstructPart corePart;

    private void SetControllingMovement(IConstructMovement movement)
    {
        if (movement == null) throw new Exception("Cannot SetControllingMovement(null).");
        if (!movements.Contains(movement)) throw new Exception("Cannot SetControllingMovement(movement) when movement not registered.");
        if (movement.IsControlling) throw new Exception("Cannot SetControllingMovement(movement) when movement already controlling.");
        if (controllingMovement != null) UnsetControllingMovement();
        controllingMovement = movement;
        controllingMovement.SetControlling();
    }

    private void UnsetControllingMovement()
    {
        if (controllingMovement == null) Utility.LogWarning("Cannot UnsetControllingMovement() when no controlled movement.");
        controllingMovement.UnsetControlling();
        controllingMovement = null;
    }
}
