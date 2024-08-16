using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public enum PartWeightClass
{ S, M, L, XL };

public enum PartTag
{ Core, Sharp };

[RequireComponent(typeof(WorldObject))]
public partial class ConstructPart : MonoBehaviour
{
    public static List<ConstructPart> GlobalParts = new List<ConstructPart>();

    public UnityAction OnPropertiesChange = delegate { };
    public WorldObject WO => worldObject;
    public Sprite Icon => icon;
    public List<PartTag> Tags => tags;
    public List<ConstructShape> Shapes => shapes;
    public PartWeightClass WeightClass { get; private set; } = PartWeightClass.S;
    public int Level { get; private set; } = 1;
    public float Health { get; private set; } = 1.0f;
    public float XP { get; private set; } = 0.6f;
    public float RequiredXP => 1.0f + Level * 0.5f;
    public float MaxHealth { get; private set; } = 1.0f;
    public Construct CurrentConstruct { get; private set; } = null;
    public IPartController CurrentController { get; private set; } = null;
    public bool IsConstructed => CurrentConstruct != null;
    public bool IsControlled => CurrentController != null;
    public bool CanControl => !IsControlled;

    public PhysicalHandle TakeControl(IPartController controller)
    {
        Assert.IsTrue(CanControl);
        CurrentController = controller;
        return new PhysicalHandle(this);
    }

    public Vector3 GetCentre()
    {
        return worldObject.transform.position;
    }

    public void OnJoinConstruct(Construct construct)
    {
        Assert.IsNull(CurrentConstruct);
        CurrentConstruct = construct;
        foreach (ConstructMovement movement in movements) construct.RegisterPartMovement(this, movement);
        foreach (ConstructSkill skill in skills) construct.RegisterPartSkill(this, skill);
        foreach (ConstructShape shape in shapes) construct.RegisterPartShape(this, shape);
    }

    public void OnLeaveConstruct(Construct construct)
    {
        Assert.IsTrue(CurrentConstruct == construct);
        foreach (ConstructMovement movement in movements) construct.UnregisterPartMovement(this, movement);
        foreach (ConstructSkill skill in skills) construct.UnregisterPartSkill(this, skill);
        foreach (ConstructShape shape in shapes) construct.UnregisterPartShape(this, shape);
        CurrentConstruct = null;
    }

    public void JoinShape(ConstructShape shape)
    {
        Assert.IsFalse(shapes.Contains(shape));
        shapes.Add(shape);
        if (IsConstructed) CurrentConstruct.RegisterPartShape(this, shape);
        OnPropertiesChange();
    }

    public void LeaveShape(ConstructShape shape)
    {
        Assert.IsTrue(shapes.Contains(shape));
        shapes.Remove(shape);
        if (IsConstructed) CurrentConstruct.UnregisterPartShape(this, shape);
        OnPropertiesChange();
    }

    private static Dictionary<PartWeightClass, float> WEIGHT_FORCE_MULT = new()
    {
        { PartWeightClass.S, 1.0f },
        { PartWeightClass.M, 0.8f },
        { PartWeightClass.L, 0.6f },
        { PartWeightClass.XL, 0.4f }
    };

    [Header("References")]
    [SerializeField] private WorldObject worldObject;
    [SerializeField] private Sprite icon;
    [SerializeField] private List<PartTag> tags = new();
    [SerializeField] private List<ConstructMovement> movements = new();
    [SerializeField] private List<ConstructSkill> skills = new();
    [SerializeField] private List<ConstructShape> shapes = new();

    private static PartWeightClass GetWeightClass(float weight)
    {
        if (weight <= 5f) return PartWeightClass.S;
        if (weight <= 25f) return PartWeightClass.M;
        return PartWeightClass.L;
    }

    private void Awake()
    {
        // Initialize physical properties
        worldObject.InitPhysical();
        WeightClass = GetWeightClass(worldObject.Weight);
        ConstructPart.GlobalParts.Add(this);
    }

    private void OnDestroy()
    {
        ConstructPart.GlobalParts.Remove(this);
    }

    private void ReleaseControl()
    {
        Assert.IsTrue(IsControlled);
        CurrentController = null;
    }

    private void AddMovement(ConstructMovement movement)
    {
        Assert.IsFalse(movements.Contains(movement));
        movements.Add(movement);
        if (IsConstructed) CurrentConstruct.RegisterPartMovement(this, movement);
        OnPropertiesChange();
    }

    private void RemoveMovement(ConstructMovement movement)
    {
        Assert.IsTrue(movements.Contains(movement));
        movements.Remove(movement);
        if (IsConstructed) CurrentConstruct.UnregisterPartMovement(this, movement);
        OnPropertiesChange();
    }

    private void AddSkill(ConstructSkill skill)
    {
        Assert.IsFalse(skills.Contains(skill));
        skills.Add(skill);
        if (IsConstructed) CurrentConstruct.RegisterPartSkill(this, skill);
        OnPropertiesChange();
    }

    private void RemoveSkill(ConstructSkill skill)
    {
        Assert.IsTrue(skills.Contains(skill));
        skills.Remove(skill);
        if (IsConstructed) CurrentConstruct.UnregisterPartSkill(this, skill);
    }

    private void OnDrawGizmos()
    {
        // TODO: Draw indicators for constructed / controlled / controlled type
    }
}

public partial class ConstructPart : MonoBehaviour
{
    public class PhysicalHandle
    {
        public bool IsValid { get; private set; } = true;
        public ConstructPart Part { get; private set; }

        public PhysicalHandle(ConstructPart part)
        {
            Part = part;
        }

        public void Release()
        {
            Assert.IsTrue(IsValid);
            ResetPhysics();
            Part.ReleaseControl();
            IsValid = false;
        }

        public void AddWeightedForce(Vector3 force)
        {
            Assert.IsTrue(IsValid);
            Part.worldObject.RB.AddForce(force * WEIGHT_FORCE_MULT[Part.WeightClass], ForceMode.VelocityChange);
        }

        public void AddWeightedTorque(Vector3 torque)
        {
            Assert.IsTrue(IsValid);
            Part.worldObject.RB.AddTorque(torque * WEIGHT_FORCE_MULT[Part.WeightClass], ForceMode.VelocityChange);
        }

        public void SetPhysicsMode(bool isKinematic, bool useGravity)
        {
            Assert.IsTrue(IsValid);
            Part.worldObject.RB.isKinematic = isKinematic;
            Part.worldObject.RB.useGravity = useGravity;
        }

        public void SetPhysicsProperties(float drag, float angularDrag)
        {
            Assert.IsTrue(IsValid);
            Part.worldObject.RB.drag = drag;
            Part.worldObject.RB.angularDrag = angularDrag;
        }

        public void SetEnableCollisions(bool enabled)
        {
            Assert.IsTrue(IsValid);
            Part.worldObject.RB.detectCollisions = enabled;
        }

        public void ResetPhysics()
        {
            Assert.IsTrue(IsValid);
            SetPhysicsMode(false, true);
            SetPhysicsProperties(0.0f, 0.0f);
            SetEnableCollisions(true);
        }
    }
}
