using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class Construct : MonoBehaviour
{
    public UnityAction OnConstructChange = delegate { };
    public List<ConstructPart> Parts { get; private set; } = new();
    public List<ConstructMovement> Movements { get; private set; } = new();
    public List<ConstructShape> Shapes { get; private set; } = new();
    public List<ConstructShape> ActiveShapes => Shapes.FindAll(shape => shape.IsConstructed);
    public List<ConstructSkill> Skills { get; private set; } = new();
    public ActionSet AssignedSkills { get; private set; } = new();
    public ConstructPart CorePart { get; private set; }
    public ConstructMovement ControllingMovement { get; private set; }

    public void InitCore(ConstructPart corePart)
    {
        if (this.CorePart != null) throw new Exception("Cannot SetCore() when already have a core.");
        this.CorePart = corePart;
        AddPart(this.CorePart);
        UpdateControllingMovement();
    }

    public void Move(Vector3 dir) => ControllingMovement?.Move(dir);

    public void Aim(Vector3 pos) => ControllingMovement?.Aim(pos);

    public void SkillInputDown(int slot) => AssignedSkills.ActionInputDown(slot);

    public void SkillInputUp(int slot) => AssignedSkills.ActionInputUp(slot);

    public void PerformConstruction((ConstructShape shape, ConstructPart part, int slot) construction)
    {
        // Ensure still a valid construction
        (bool, int) canConstruct = construction.shape.CanConstructWith(construction.part);
        if (!canConstruct.Item1) return;
        if (canConstruct.Item2 != construction.slot) return;

        // Add part (and in turn the shape) if not already
        // The shape is either on the construct or on one the new parts
        foreach (ConstructPart part in construction.shape.Parts)
        {
            if (!Parts.Contains(part)) AddPart(part);
        }

        // Perform construction
        construction.shape.ConstructWith(construction.part, canConstruct.Item2);

        OnConstructChange();
    }

    public void EnterForging()
    {
    }

    public void ExitForging()
    {
    }

    public void UpdateControllingMovement()
    {
        // Unassign if no longer controlling
        if (ControllingMovement != null)
        {
            if (!ControllingMovement.IsControlling)
            {
                ControllingMovement = null;
                ControllingMovement.OnChangeControlling -= UpdateControllingMovement;
            }
            else return;
        }

        // Assign first possible movement
        foreach (ConstructMovement movement in Movements)
        {
            if (movement.CanSetControlling())
            {
                SetControllingMovement(movement);
                return;
            }
        }
    }

    private void SetControllingMovement(ConstructMovement movement)
    {
        if (!Movements.Contains(movement)) throw new Exception("Cannot SetControllingMovement(movement) when movement not registered.");
        if (movement.IsControlling) throw new Exception("Cannot SetControllingMovement(movement) when movement already controlling.");
        if (ControllingMovement != null) UnsetControllingMovement();
        ControllingMovement = movement;
        ControllingMovement.OnChangeControlling += UpdateControllingMovement;
        ControllingMovement.SetControlling();
    }

    private void UnsetControllingMovement()
    {
        if (ControllingMovement == null) Utility.LogWarning("Cannot UnsetControllingMovement() when no controlled movement.");
        ControllingMovement.UnsetControlling();
        ControllingMovement.OnChangeControlling -= UpdateControllingMovement;
        ControllingMovement = null;
    }

    public void AddPart(ConstructPart part)
    {
        if (part.IsConstructed) throw new Exception("Cannot AddPart(part) when part already constructed.");
        Parts.Add(part);
        part.OnJoinConstruct(this);
        OnConstructChange();
    }

    public void RemovePart(ConstructPart part)
    {
        if (!part.IsConstructed) throw new Exception("Cannot RemovePart(part) when part not constructed.");
        if (!Parts.Contains(part)) throw new Exception("Cannot RemovePart(part), not registered!");
        Parts.Remove(part);
        part.OnLeaveConstruct(this);
        OnConstructChange();
    }

    public Vector3 GetCentre()
    {
        if (ControllingMovement == null) return CorePart.GetCentre();
        return ControllingMovement.GetCentre();
    }

    public void OnRegisterMovement(ConstructMovement movement)
    {
        if (Movements.Contains(movement)) throw new Exception("Cannot RegisterPartMovement(movement), already registered!");
        Movements.Add(movement);
        OnConstructChange();
    }

    public void OnUnregisterMovement(ConstructMovement movement)
    {
        if (!Movements.Contains(movement)) throw new Exception("Cannot UnregisterPartMovement(movement), not registered!");
        if (ControllingMovement == movement) UnsetControllingMovement();
        Movements.Remove(movement);
        OnConstructChange();
    }

    public void OnRegisterSkill(ConstructSkill skill, int slot = -1)
    {
        if (Skills.Contains(skill)) throw new Exception("RegisterSkill(skill): already registered.");
        Skills.Add(skill);
        if (AssignedSkills.AvailableSlotCount > 0) AssignedSkills.RegisterAction(skill, slot);
        OnConstructChange();
    }

    public void OnUnregisterSkill(ConstructSkill skill)
    {
        if (!Skills.Contains(skill)) throw new Exception("UnregisterSkill(skill): not registered.");
        if (skill.IsAssigned) AssignedSkills.UnregisterAction(skill);
        Skills.Remove(skill);
        OnConstructChange();
    }

    public void OnRegisterShape(ConstructShape shape)
    {
        // Do not error on duplicate shape as the following logic is valid:
        // Perform Construction -> Add other part -> shape is added
        //                      -> construction happens -> part is added to shape -> shape is added
        if (Shapes.Contains(shape)) return;
        Shapes.Add(shape);
        OnConstructChange();
    }

    public void OnUnregisterShape(ConstructShape shape)
    {
        if (Shapes.Contains(shape)) throw new Exception("UnregisterShape(shape): not registered.");
        Shapes.Remove(shape);
        OnConstructChange();
    }
}
