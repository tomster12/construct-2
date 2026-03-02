using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

[Serializable]
public struct PartConstruction
{
    public ConstructShape shape;
    public ConstructPart part;
    public int slot;
}

public class Construct : MonoBehaviour
{
    public enum EventType
    { Add, Remove }

    public UnityAction<EventType, ConstructPart> OnPartEvent = delegate { };
    public UnityAction<EventType, ConstructSkill> OnSkillEvent = delegate { };
    public UnityAction<EventType, ConstructMovement> OnMovementEvent = delegate { };
    public UnityAction<EventType, ConstructShape, ConstructPart> OnShapeEvent = delegate { };
    public UnityAction<EventType, ConstructShape, ConstructPart> OnActiveShapeEvent = delegate { };
    public UnityAction<ConstructMovement> OnControllingMovementChange = delegate { };
    public List<ConstructPart> Parts => parts;
    public bool IsInitialized => corePart != null;

    private ConstructPart corePart;
    private List<ConstructPart> parts = new();
    private List<ConstructMovement> subscribedMovements = new();
    private List<ConstructShape> subscribedShapes = new();
    private List<ConstructSkill> registeredSkills = new();
    private ActionSet assignedSkills = new();
    private ConstructMovement controllingMovement;
    private bool isConstructing;

    // ---------------- Lifetime ----------------

    public void InitCore(ConstructPart corePart)
    {
        Assert.IsTrue(corePart != null);
        Assert.IsTrue(this.corePart == null);
        
        this.corePart = corePart;
        AddPart(this.corePart);
    }

    private void OnDestroy()
    {
        // TODO: Stop listening to all events, constructs lifetimes will not be game wide
    }

    private void LateUpdate()
    {
        // After dust has settled then try and update if we need to change the controlling movement
        CheckAndAssignControllingMovement();
    }

    // ---------------- Main ----------------

    public void Move(Vector3 dir) => controllingMovement?.Move(dir);

    public void Aim(Vector3 pos) => controllingMovement?.Aim(pos);

    public void SkillInputDown(int slot) => assignedSkills.ActionInputDown(slot);

    public void SkillInputUp(int slot) => assignedSkills.ActionInputUp(slot);

    public async Task TryConstructPart(PartConstruction construction)
    {
        if (isConstructing) return;

        (bool, int) canConstruct = construction.shape.CanAddPart(construction.part);

        if (!canConstruct.Item1) return;
        if (canConstruct.Item2 != construction.slot) return;

        isConstructing = true;

        // Expected behaviour:
        // - Shape / movement moves the parts into position
        // - All the parts involved are set to active parts
        // - By the time all parts are added we should have added the shape (in RegisterConstructPartActiveShape())
        // - Construction ends and OnShapePartsChange() is called
        // - Here we add all the parts to the construct
        // - Any parts outside the construct should be dependant on the shape

        bool success = await construction.shape.AddPart(construction.part, canConstruct.Item2);
        Assert.IsTrue(success);

        isConstructing = false;

        // We can assume that during the construction process one the following will have happened:
        // - RegisterConstructShape() called because our part joined another parts shape
        // - OnShapePartsChange() called because another part joined our parts shape
        // Eitherway all the parts and shapes from everyone involved should have been added to this construct

        Assert.IsTrue(subscribedShapes.Contains(construction.shape));
        foreach (ConstructPart part in construction.shape.Parts) Assert.IsTrue(parts.Contains(part));
    }

    public void TryRemovePart(ConstructPart part)
    {
        if (isConstructing) return;
        Assert.IsTrue(parts.Contains(part));
        Assert.IsFalse(part == corePart);
        RemovePart(part);
    }

    public async Task TryDeconstruct()
    {
        if (isConstructing) return;
        await corePart.Deconstruct();
    }

    private void CheckAndAssignControllingMovement()
    {
        if (controllingMovement != null) return;

        // Assign first possible movement
        foreach (ConstructMovement movement in subscribedMovements)
        {
            if (movement.CanActivate())
            {
                controllingMovement = movement;
                controllingMovement.Activate();
                controllingMovement.OnStateChanged += OnControllingMovementStateChanged;
                OnControllingMovementChange(controllingMovement);
                break;
            }
        }
    }

    private void UnsetControllingMovement()
    {
        // TODO: This seems to be recursive
        if (controllingMovement.IsActive) controllingMovement.Deactivate();

        controllingMovement.OnStateChanged -= OnControllingMovementStateChanged; // This line is being called twice and silently failing
        controllingMovement = null;
        OnControllingMovementChange(null);
    }
    
    public PartConstruction[] GetAvailableConstructions(ConstructPart targetPart)
    {
        // We want all constructions either on the construct or on the shape
        List<PartConstruction> AvailableConstructions = new();

        // Find which of our shapes the targetted parts fits with
        foreach (ConstructShape shape in subscribedShapes)
        {
            (bool canConstruct, int slot) = shape.CanAddPart(targetPart);
            if (canConstruct) AvailableConstructions.Add(new PartConstruction { shape = shape, part = targetPart, slot = slot });
        }

        // Find which shapes in the target part which some of our parts fit with
        foreach (ConstructShape shape in targetPart.InherentShapes)
        {
            foreach (ConstructPart part in parts)
            {
                (bool canConstruct, int slot) = shape.CanAddPart(part);
                if (canConstruct) AvailableConstructions.Add(new PartConstruction { shape = shape, part = part, slot = slot });
            }
        }

        return AvailableConstructions.ToArray();
    }

    public Vector3 GetCentre()
    {
        if (controllingMovement == null) return corePart.GetCentre();
        return controllingMovement.GetCentre();
    }

    // ---------------- Registration ----------------

    private void AddPart(ConstructPart part)
    {
        Assert.IsFalse(part.IsConstructed);
        Assert.IsFalse(parts.Contains(part));

        // This is either a core part, or added due to a shape being added / changed
        // Eitherway we just need to update the construct with all the parts components

        part.NotifyAddedToConstruct(this);
        parts.Add(part);
        OnPartEvent(EventType.Add, part);

        foreach (ConstructSkill skill in part.Skills) RegisterPartSkill(part, skill);
        foreach (ConstructMovement movement in part.Movements) RegisterPartMovement(part, movement);
        foreach (ConstructShape shape in part.InherentShapes) RegisterPartShape(part, shape);

        part.OnSkillEvent += OnPartSkillEvent;
        part.OnMovementEvent += OnPartMovementEvent;
        part.OnShapeParticipationEvent += OnPartShapeParticipationEvent;
    }

    private void RemovePart(ConstructPart part)
    {
        Assert.IsTrue(part.IsConstructed);
        Assert.IsTrue(parts.Contains(part));
        Assert.IsFalse(part == corePart);

        parts.Remove(part);
        part.NotifyRemovedFromConstruct(this);
        OnPartEvent(EventType.Remove, part);

        foreach (ConstructSkill skill in part.Skills) UnregisterPartSkill(part, skill);
        foreach (ConstructMovement movement in part.Movements) UnregisterPartMovement(part, movement);
        foreach (ConstructShape shape in part.InherentShapes) UnregisterPartShape(part, shape);

        part.OnSkillEvent -= OnPartSkillEvent;
        part.OnMovementEvent -= OnPartMovementEvent;
        part.OnShapeParticipationEvent -= OnPartShapeParticipationEvent;
    }

    private void RegisterPartSkill(ConstructPart part, ConstructSkill skill)
    {
        Assert.IsTrue(parts.Contains(part));
        Assert.IsFalse(registeredSkills.Contains(skill));

        registeredSkills.Add(skill);
        if (assignedSkills.AvailableSlotCount > 0) assignedSkills.RegisterAction(skill);

        OnSkillEvent(EventType.Add, skill);
    }

    private void UnregisterPartSkill(ConstructPart part, ConstructSkill skill)
    {
        Assert.IsTrue(registeredSkills.Contains(skill));

        registeredSkills.Remove(skill);
        if (skill.IsAssigned) assignedSkills.UnregisterAction(skill);

        OnSkillEvent(EventType.Remove, skill);
    }

    private void RegisterPartMovement(ConstructPart part, ConstructMovement movement)
    {
        Assert.IsTrue(parts.Contains(part));
        Assert.IsFalse(subscribedMovements.Contains(movement));

        subscribedMovements.Add(movement);

        OnMovementEvent(EventType.Add, movement);
    }

    private void UnregisterPartMovement(ConstructPart part, ConstructMovement movement)
    {
        Assert.IsTrue(subscribedMovements.Contains(movement));

        if (controllingMovement == movement) movement.Deactivate();

        subscribedMovements.Remove(movement);
        OnMovementEvent(EventType.Remove, movement);
    }

    private void RegisterPartShape(ConstructPart part, ConstructShape shape)
    {
        Assert.IsTrue(parts.Contains(part));

        // A part has been added therefore subscribe its shapes
        // It is possible its shape has already been added during an existing part becoming active
        if (!subscribedShapes.Contains(shape))
        {
            shape.OnPartEvent += OnShapePartEvent;
            subscribedShapes.Add(shape);
            OnShapeEvent(EventType.Add, shape, part);
        }
    }

    private void UnregisterPartShape(ConstructPart part, ConstructShape shape)
    {
        Assert.IsTrue(subscribedShapes.Contains(shape));

        shape.OnPartEvent -= OnShapePartEvent;
        subscribedShapes.Remove(shape);
        OnShapeEvent(EventType.Remove, shape, part);
    }

    private void HandlePartJoiningShape(ConstructPart part, ConstructShape shape)
    {
        Assert.IsTrue(parts.Contains(part));

        // A part we control has been set as active on a new shape (potentially outside the construct)
        // Track the shape if needed, and then deal with new parts in OnShapePartsChange()
        if (!subscribedShapes.Contains(shape))
        {
            subscribedShapes.Add(shape);
            shape.OnPartEvent += OnShapePartEvent;
            OnShapeEvent(EventType.Add, shape, part);
        }

        // Our parts inherent shape is being activated with a new part from outside the construct
        // We can just do nothing and wait for OnShapePartsChange() to be called
        else { }

        // Eitherway this part has been added to this shape so notify
        OnActiveShapeEvent(EventType.Add, shape, part);
    }

    private void HandlePartLeavingShape(ConstructPart part, ConstructShape shape)
    {
        Assert.IsTrue(subscribedShapes.Contains(shape));

        // If the part is dependant on the shape then we need to remove it from the construct
        // The part should already have removed the shape from itself so this shouldn't recurse
        // See OnShapePartChange() for shape removal and clean up
        if (part.ConstructionDependantShape == shape)
        {
            RemovePart(part);
        }
    }
    
    // ---------------- Events ----------------

    private void OnControllingMovementStateChanged(ConstructMovement movement, bool isActive)
    {
        if (!isActive) UnsetControllingMovement();
    }

    private void OnPartSkillEvent(ConstructPart part, ConstructPart.EventType type, ConstructSkill skill)
    {
        if (type == ConstructPart.EventType.Add) RegisterPartSkill(part, skill);
        else if (type == ConstructPart.EventType.Remove) UnregisterPartSkill(part, skill);
    }

    private void OnPartMovementEvent(ConstructPart part, ConstructPart.EventType type, ConstructMovement movement)
    {
        if (type == ConstructPart.EventType.Add) RegisterPartMovement(part, movement);
        else if (type == ConstructPart.EventType.Remove) UnregisterPartMovement(part, movement);
    }

    private void OnPartShapeParticipationEvent(ConstructPart part, ConstructPart.EventType type, ConstructShape shape)
    {
        if (type == ConstructPart.EventType.Add) HandlePartJoiningShape(part, shape);
        else if (type == ConstructPart.EventType.Remove) HandlePartLeavingShape(part, shape);
    }

    private void OnShapePartEvent(ConstructShape shape, bool isAdded)
    {
        Assert.IsTrue(subscribedShapes.Contains(shape));

        // This will be called after all the parts have been added / removed to the shape

        // If shapes have been added then this is the key point we add them to the construct
        if (isAdded)
        {
            foreach (ConstructPart part in shape.Parts)
            {
                if (!parts.Contains(part)) AddPart(part);
            }
        }

        // If a part is removed from a shape then HandlePartLeavingShape() will remove it from the construct if necessary
        // We can presume shapes will be deconstructed and removed when their inherent part is removed
        // It is possible that a shape removes the part that connects it to the core but does not deconstruct
        // Therefore here we check that atleast one of the parts of the shape is dependant on something other than the shape
        else if (!isAdded)
        {
            bool isConnected = false;
            foreach (ConstructPart part in shape.Parts)
            {
                if (part.ConstructionDependantShape != shape)
                {
                    isConnected = true;
                    break;
                }
            }
            if (!isConnected)
            {
                subscribedShapes.Remove(shape);
                shape.OnPartEvent -= OnShapePartEvent;
                OnShapeEvent(EventType.Remove, shape, null);

                // This is a complex but possible control flow to reach this point
                // Just for now throw an exception so we can check this is happening properly
                // Can remove this once confirmed this is reasonably expected
                throw new System.Exception("Shape is not connected to the construct");
            }
        }
    }
}
