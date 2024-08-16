using Mono.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public enum ConstructState
{
    Active, Forging, Inactive
}

public struct Construction
{
    public ConstructShape shape;
    public ConstructPart part;
    public int slot;
}

public class Construct : MonoBehaviour
{
    public UnityAction OnConstructChange = delegate { };
    public UnityAction<ConstructState> OnStateChangeChange = delegate { };
    public List<ConstructPart> Parts { get; private set; } = new();
    public Dictionary<ConstructMovement, ConstructPart> SubscribedMovements { get; private set; } = new();
    public Dictionary<ConstructShape, List<ConstructPart>> SubscribedShapes { get; private set; } = new();
    public Dictionary<ConstructSkill, ConstructPart> SubscribedSkills { get; private set; } = new();
    public ActionSet AssignedSkills { get; private set; } = new();
    public ConstructPart CorePart { get; private set; }
    public ConstructMovement PrimaryMovement { get; private set; }
    public ConstructState State { get; private set; } = ConstructState.Inactive;

    public void InitCore(ConstructPart corePart)
    {
        Assert.IsTrue(corePart != null);
        CorePart = corePart;
        AddPart(CorePart);
        State = ConstructState.Active;
        UpdatePrimaryMovement();
    }

    public void Move(Vector3 dir) => PrimaryMovement?.Move(dir);

    public void Aim(Vector3 pos) => PrimaryMovement?.Aim(pos);

    public void SkillInputDown(int slot) => AssignedSkills.ActionInputDown(slot);

    public void SkillInputUp(int slot) => AssignedSkills.ActionInputUp(slot);

    public void PerformConstruction(Construction construction)
    {
        // Ensure still a valid construction
        (bool, int) canConstruct = construction.shape.CanConstructWith(construction.part);
        if (!canConstruct.Item1) return;
        if (canConstruct.Item2 != construction.slot) return;

        // TODO: Figure if this should be allowed
        // Construct -> attaching adds shape -> construct gets shape
        //           -> add attachee part    -> construct gets shape

        // Begin async construction
        StartCoroutine(construction.shape.EnumConstructWith(construction.part, canConstruct.Item2, (bool success) =>
        {
            // TODO: Implement cancelling
            Assert.IsTrue(success);

            // Target part is outside the construct, therefore shape is part of construct, so only add part
            if (!Parts.Contains(construction.part))
            {
                AddPart(construction.part);
            }

            // Target part is part of the construct, therefore shape is outside so add each shape parts (and in turn the shape)
            else
            {
                foreach (ConstructPart part in construction.shape.Parts)
                {
                    if (!Parts.Contains(part)) AddPart(part);
                }
            }

            OnConstructChange();
        }));
    }

    public Construction[] GetAvailableConstructions(ConstructPart targetPart)
    {
        List<Construction> AvailableConstructions = new();

        // Find all construct shapes that targetted parts fits with
        foreach (ConstructShape shape in SubscribedShapes.Keys)
        {
            (bool canConstruct, int slot) = shape.CanConstructWith(targetPart);
            if (canConstruct) AvailableConstructions.Add(new Construction { shape = shape, part = targetPart, slot = slot });
        }

        // Find all shapes  in the target part which some construct part fits with
        foreach (ConstructShape shape in targetPart.Shapes)
        {
            foreach (ConstructPart part in Parts)
            {
                (bool canConstruct, int slot) = shape.CanConstructWith(part);
                if (canConstruct) AvailableConstructions.Add(new Construction { shape = shape, part = part, slot = slot });
            }
        }

        return AvailableConstructions.ToArray();
    }

    public bool CanSetState(ConstructState state)
    {
        switch (state)
        {
            case ConstructState.Active:
                return true;

            case ConstructState.Forging:
                return State == ConstructState.Active;

            case ConstructState.Inactive:
                return State == ConstructState.Active;

            default:
                Assert.IsTrue(false);
                return false;
        }
    }

    public void SetState(ConstructState state)
    {
        Assert.IsTrue(CanSetState(state));
        State = state;
        OnStateChangeChange(State);
    }

    public Vector3 GetCentre()
    {
        if (PrimaryMovement == null) return CorePart.GetCentre();
        return PrimaryMovement.GetCentre();
    }

    public void AddPart(ConstructPart part)
    {
        Assert.IsFalse(part.IsConstructed);
        Parts.Add(part);
        part.OnJoinConstruct(this);
        OnConstructChange();
    }

    public void RemovePart(ConstructPart part)
    {
        Assert.IsTrue(part.IsConstructed);
        Assert.IsTrue(Parts.Contains(part));
        Parts.Remove(part);
        part.OnLeaveConstruct(this);
        OnConstructChange();
    }

    public void RegisterPartMovement(ConstructPart part, ConstructMovement movement)
    {
        Assert.IsFalse(SubscribedMovements.ContainsKey(movement));
        Assert.IsTrue(Parts.Contains(part));
        SubscribedMovements.Add(movement, part);
        OnConstructChange();
    }

    public void UnregisterPartMovement(ConstructPart part, ConstructMovement movement)
    {
        Assert.IsTrue(SubscribedMovements.ContainsKey(movement));
        SubscribedMovements.Remove(movement);
        OnConstructChange();
    }

    public void RegisterPartSkill(ConstructPart part, ConstructSkill skill, int slot = -1)
    {
        Assert.IsFalse(SubscribedSkills.ContainsKey(skill));
        Assert.IsTrue(Parts.Contains(part));
        SubscribedSkills.Add(skill, part);
        if (AssignedSkills.AvailableSlotCount > 0 && slot != -1) AssignedSkills.RegisterAction(skill, slot);
        OnConstructChange();
    }

    public void UnregisterPartSkill(ConstructPart part, ConstructSkill skill)
    {
        Assert.IsTrue(SubscribedSkills.ContainsKey(skill));
        SubscribedSkills.Remove(skill);
        AssignedSkills.UnregisterAction(skill);
        OnConstructChange();
    }

    public void RegisterPartShape(ConstructPart part, ConstructShape shape)
    {
        Assert.IsTrue(Parts.Contains(part));
        if (SubscribedShapes.ContainsKey(shape)) Assert.IsFalse(SubscribedShapes[shape].Contains(part));
        if (!SubscribedShapes.ContainsKey(shape)) SubscribedShapes.Add(shape, new());
        SubscribedShapes[shape].Add(part);
        OnConstructChange();
    }

    public void UnregisterPartShape(ConstructPart part, ConstructShape shape)
    {
        Assert.IsTrue(SubscribedShapes.ContainsKey(shape));
        Assert.IsTrue(SubscribedShapes[shape].Contains(part));
        SubscribedShapes[shape].Remove(part);
        if (SubscribedShapes[shape].Count == 0) SubscribedShapes.Remove(shape);
        OnConstructChange();
    }

    private void Update()
    {
        UpdatePrimaryMovement();
    }

    private void UpdatePrimaryMovement()
    {
        if (PrimaryMovement != null) return;

        // Assign first possible movement
        foreach (ConstructMovement movement in SubscribedMovements.Keys)
        {
            if (movement.CanActivate())
            {
                PrimaryMovement = movement;
                PrimaryMovement.Activate();
                PrimaryMovement.OnStateChange += OnPrimaryMovementStateChange;
                break;
            }
        }
    }

    private void OnPrimaryMovementStateChange(bool isActive)
    {
        if (!isActive) UnsetPrimaryMovement();
    }

    private void UnsetPrimaryMovement()
    {
        if (PrimaryMovement.IsActive) PrimaryMovement.Deactivate();
        PrimaryMovement.OnStateChange -= OnPrimaryMovementStateChange;
        PrimaryMovement = null;
    }
}
