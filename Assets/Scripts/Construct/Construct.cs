using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public struct Construction
{
    public ConstructShape shape;
    public ConstructPart part;
    public int slot;
}

public class Construct : MonoBehaviour
{
    public UnityAction<EventType, ConstructPart> OnPartEvent = delegate { };
    public UnityAction<EventType, ConstructMovement> OnMovementEvent = delegate { };
    public UnityAction<EventType, ConstructSkill> OnSkillEvent = delegate { };
    public UnityAction<EventType, ConstructShape> OnShapeEvent = delegate { };
    public UnityAction<State> OnStateChange = delegate { };

    public enum State
    {
        Active, Forging, Inactive
    }

    public enum EventType
    {
        Add, Remove, Change
    }

    public List<ConstructPart> Parts => parts;

    public void InitCore(ConstructPart corePart)
    {
        Assert.IsTrue(corePart != null);
        currentState = State.Active;
        this.corePart = corePart;
        AddPart(this.corePart);
    }

    public void AddPart(ConstructPart part) // Expects caller to be self
    {
        Assert.IsFalse(part.IsConstructed);

        parts.Add(part);

        foreach (ConstructSkill skill in part.Skills) RegisterConstructSkill(part, skill);
        foreach (ConstructMovement movement in part.Movements) RegisterConstructMovement(part, movement);
        foreach (ConstructShape shape in part.Shapes) RegisterConstructShape(part, shape);

        part.OnSkillEvent += OnPartSkillEvent;
        part.OnMovementEvent += OnPartMovementEvent;
        part.OnShapeEvent += OnPartShapeEvent;

        part.JoinConstruct(this);

        OnPartEvent(EventType.Add, part);
    }

    public void RemovePart(ConstructPart part) // Expects caller to be self
    {
        Assert.IsTrue(part.IsConstructed);
        Assert.IsTrue(parts.Contains(part));

        foreach (ConstructSkill skill in part.Skills) UnregisterConstructSkill(part, skill);
        foreach (ConstructMovement movement in part.Movements) UnregisterConstructMovement(part, movement);
        foreach (ConstructShape shape in part.Shapes) UnregisterConstructShape(part, shape);

        part.OnSkillEvent -= OnPartSkillEvent;
        part.OnMovementEvent -= OnPartMovementEvent;
        part.OnShapeEvent -= OnPartShapeEvent;

        parts.Remove(part);
        part.LeaveConstruct(this);

        OnPartEvent(EventType.Remove, part);
    }

    public void Move(Vector3 dir) => primaryMovement?.Move(dir);

    public void Aim(Vector3 pos) => primaryMovement?.Aim(pos);

    public void SkillInputDown(int slot) => assignedSkills.ActionInputDown(slot);

    public void SkillInputUp(int slot) => assignedSkills.ActionInputUp(slot);

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
        StartCoroutine(construction.shape.EnumConstructWith(construction.part, canConstruct.Item2, (System.Action<bool>)((bool success) =>
        {
            // TODO: Implement cancelling
            Assert.IsTrue(success);

            // Target part is outside the construct, therefore shape is part of construct, so only add part
            if (!this.parts.Contains(construction.part))
            {
                AddPart(construction.part);
            }

            // Target part is part of the construct, therefore shape is outside so add each shape parts (and in turn the shape)
            else
            {
                foreach (ConstructPart part in construction.shape.Parts)
                {
                    if (!this.parts.Contains(part)) AddPart(part);
                }
            }

            OnShapeEvent(EventType.Add, construction.shape);
        })));
    }

    public Construction[] GetAvailableConstructions(ConstructPart targetPart)
    {
        // We want all constructions either on the construct or on the shape
        List<Construction> AvailableConstructions = new();

        // Find which of our shapes the targetted parts fits with
        foreach (ConstructShape shape in subscribedShapes.Keys)
        {
            (bool canConstruct, int slot) = shape.CanConstructWith(targetPart);
            if (canConstruct) AvailableConstructions.Add(new Construction { shape = shape, part = targetPart, slot = slot });
        }

        // Find which shapes in the target part which some of our parts fit with
        foreach (ConstructShape shape in targetPart.Shapes)
        {
            foreach (ConstructPart part in parts)
            {
                (bool canConstruct, int slot) = shape.CanConstructWith(part);
                if (canConstruct) AvailableConstructions.Add(new Construction { shape = shape, part = part, slot = slot });
            }
        }

        return AvailableConstructions.ToArray();
    }

    public bool CanSetState(State state)
    {
        switch (state)
        {
            case State.Active:
                return true;

            case State.Forging:
                return currentState == State.Active;

            case State.Inactive:
                return currentState == State.Active;

            default:
                Assert.IsTrue(false);
                return false;
        }
    }

    public void SetState(State state)
    {
        Assert.IsTrue(CanSetState(state));
        currentState = state;
        OnStateChange(currentState);
    }

    public Vector3 GetCentre()
    {
        if (primaryMovement == null) return corePart.GetCentre();
        return primaryMovement.GetCentre();
    }

    private State currentState = State.Inactive;
    private ConstructPart corePart;
    private List<ConstructPart> parts = new();
    private Dictionary<ConstructMovement, ConstructPart> subscribedMovements = new();
    private Dictionary<ConstructShape, List<ConstructPart>> subscribedShapes = new();
    private Dictionary<ConstructSkill, ConstructPart> subscribedSkills = new();
    private ActionSet assignedSkills = new();
    private ConstructMovement primaryMovement;

    private void OnDestroy()
    {
        // TODO: Stop listening to all events, constructs lifetimes are not game wide
    }

    private void Update()
    {
        // TODO: We might need to update primary movement and also look at assigned skills here?
    }

    private void UpdatePrimaryMovement()
    {
        if (primaryMovement != null) return;

        // Assign first possible movement
        foreach (ConstructMovement movement in subscribedMovements.Keys)
        {
            if (movement.CanActivate())
            {
                primaryMovement = movement;
                primaryMovement.Activate();
                primaryMovement.OnStateChange += OnPrimaryMovementStateChange;
                break;
            }
        }
    }

    private void UnsetPrimaryMovement()
    {
        if (primaryMovement.IsActive) primaryMovement.Deactivate();
        primaryMovement.OnStateChange -= OnPrimaryMovementStateChange;
        primaryMovement = null;
    }

    private void RegisterConstructSkill(ConstructPart part, ConstructSkill skill)
    {
        Assert.IsFalse(subscribedSkills.ContainsKey(skill));
        Assert.IsTrue(parts.Contains(part));
        subscribedSkills.Add(skill, part);
        if (assignedSkills.AvailableSlotCount > 0) assignedSkills.RegisterAction(skill);
        OnSkillEvent(EventType.Add, skill);
    }

    private void UnregisterConstructSkill(ConstructPart part, ConstructSkill skill)
    {
        Assert.IsTrue(subscribedSkills.ContainsKey(skill));
        subscribedSkills.Remove(skill);
        assignedSkills.UnregisterAction(skill);
        OnSkillEvent(EventType.Remove, skill);
    }

    private void RegisterConstructMovement(ConstructPart part, ConstructMovement movement)
    {
        Assert.IsFalse(subscribedMovements.ContainsKey(movement));
        Assert.IsTrue(parts.Contains(part));
        subscribedMovements.Add(movement, part);
        OnMovementEvent(EventType.Add, movement);
        UpdatePrimaryMovement();
    }

    private void UnregisterConstructMovement(ConstructPart part, ConstructMovement movement)
    {
        Assert.IsTrue(subscribedMovements.ContainsKey(movement));
        subscribedMovements.Remove(movement);
        OnMovementEvent(EventType.Remove, movement);
        UpdatePrimaryMovement();
    }

    private void RegisterConstructShape(ConstructPart part, ConstructShape shape)
    {
        Assert.IsTrue(parts.Contains(part));
        bool newShape = !subscribedShapes.ContainsKey(shape);
        if (!newShape) Assert.IsFalse(subscribedShapes[shape].Contains(part));
        if (newShape) subscribedShapes.Add(shape, new());
        subscribedShapes[shape].Add(part);
        if (newShape) OnShapeEvent(EventType.Add, shape);
        else OnShapeEvent(EventType.Change, shape);
    }

    private void UnregisterConstructShape(ConstructPart part, ConstructShape shape)
    {
        Assert.IsTrue(subscribedShapes.ContainsKey(shape));
        Assert.IsTrue(subscribedShapes[shape].Contains(part));
        subscribedShapes[shape].Remove(part);
        if (subscribedShapes[shape].Count == 0)
        {
            subscribedShapes.Remove(shape);
            OnShapeEvent(EventType.Remove, shape);
        }
        else OnShapeEvent(EventType.Change, shape);
    }

    private void OnPrimaryMovementStateChange(bool isActive)
    {
        if (!isActive) UnsetPrimaryMovement();
    }

    private void OnPartSkillEvent(ConstructPart part, ConstructPart.EventType type, ConstructSkill skill)
    {
        if (type == ConstructPart.EventType.Add) RegisterConstructSkill(part, skill);
        else if (type == ConstructPart.EventType.Remove) UnregisterConstructSkill(part, skill);
    }

    private void OnPartMovementEvent(ConstructPart part, ConstructPart.EventType type, ConstructMovement movement)
    {
        if (type == ConstructPart.EventType.Add) RegisterConstructMovement(part, movement);
        else if (type == ConstructPart.EventType.Remove) UnregisterConstructMovement(part, movement);
        UpdatePrimaryMovement();
    }

    private void OnPartShapeEvent(ConstructPart part, ConstructPart.EventType type, ConstructShape shape)
    {
        if (type == ConstructPart.EventType.Add) RegisterConstructShape(part, shape);
        else if (type == ConstructPart.EventType.Remove) UnregisterConstructShape(part, shape);
    }
}
