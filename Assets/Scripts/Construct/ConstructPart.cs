using System.Collections.Generic;
using System.Threading.Tasks;
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
    public enum EventType
    { Add, Remove }

    public WorldObject WO => worldObject;
    public List<PartTag> Tags => tags;
    public PartWeightClass WeightClass => weightClass;
    public int Level => level;
    public List<ConstructMovement> Movements => movements;
    public List<ConstructShape> InherentShapes => shapes;
    public List<ConstructShape> ActiveShapes => activeShapes;
    public ConstructShape ConstructionDependantShape => constructionDependantShape;
    public List<ConstructSkill> Skills => skills;
    public IPartController CurrentController => controller;
    public bool IsConstructed => construct != null;
    public bool IsControlled => CurrentController != null;
    public bool CanControl => !IsControlled;

    public static List<ConstructPart> GlobalParts = new();

    public UnityAction<ConstructPart, EventType, Construct> OnConstructEvent = delegate { };
    public UnityAction<ConstructPart, EventType, ConstructShape> OnShapeParticipationEvent = delegate { };
    public UnityAction<ConstructPart, EventType, ConstructMovement> OnMovementEvent = delegate { };
    public UnityAction<ConstructPart, EventType, ConstructSkill> OnSkillEvent = delegate { };

    private static Dictionary<PartWeightClass, float> WEIGHT_FORCE_MULT = new()
    {
        { PartWeightClass.S, 1.0f },
        { PartWeightClass.M, 0.8f },
        { PartWeightClass.L, 0.6f },
        { PartWeightClass.XL, 0.4f }
    };

    [Header("References")]
    [SerializeField] private WorldObject worldObject;
    [SerializeField] private List<PartTag> tags = new();
    [SerializeField] private List<ConstructMovement> movements = new();
    [SerializeField] private List<ConstructSkill> skills = new();
    [SerializeField] private List<ConstructShape> shapes = new();

    private Construct construct = null;
    private IPartController controller = null;
    private PartWeightClass weightClass = PartWeightClass.S;
    private int level = 1;
    private float health = 1.0f;
    private float xp = 0.6f;
    private float maxHealth = 1.0f;
    private float RequiredXP => 1.0f + level * 0.5f;
    private List<ConstructShape> activeShapes = new();
    private ConstructShape constructionDependantShape = null;

    public async Task Deconstruct()
    {
        // If this shape is not dependant on a shape then we the core part of a shape
        // We only set construct to null if we are dependant on a shape
        if (constructionDependantShape != null)
        {
            construct = null;
            constructionDependantShape = null;
        }

        // Tell all shapes we involved in (not inherent) we are leaving
        // This should call NotifyRemovedFromShape() on this part from each of the shapes
        foreach (var shape in activeShapes)
        {
            if (!shapes.Contains(shape))
            {
                await shape.RemovePart(this);
                Assert.IsFalse(activeShapes.Contains(shape));
            }
        }

        // Similarly tell all of our inherent shapes to deconstruct
        foreach (var shape in shapes)
        {
            await shape.Deconstruct();
        }
    }

    public PhysicalHandle TakeControl(IPartController controller)
    {
        Assert.IsTrue(CanControl);
        this.controller = controller;
        return new PhysicalHandle(this);
    }

    public void NotifyAddedToConstruct(Construct construct) // Expects caller to be Construct
    {
        Assert.IsNull(this.construct);
        this.construct = construct;
        OnConstructEvent(this, EventType.Add, construct);
    }

    public void NotifyRemovedFromConstruct(Construct construct) // Expects caller to be Construct
    {
        Assert.IsTrue(this.construct == construct);
        _ = Deconstruct();
        OnConstructEvent(this, EventType.Remove, construct);
    }

    public void NotifyAddedToShape(ConstructShape shape) // Expects caller to be ConstructShape
    {
        // Can not be added to an existing shape
        Assert.IsFalse(activeShapes.Contains(shape));

        // If we are not constructed presume that we will be constructed by this shape
        // This should happen on the OnShapePartsChange() callback
        if (!IsConstructed)
        {
            Assert.IsNull(constructionDependantShape);
            constructionDependantShape = shape;
        }

        activeShapes.Add(shape);
        OnShapeParticipationEvent(this, EventType.Add, shape);
    }

    public void NotifyRemovedFromShape(ConstructShape shape) // Expects caller to be ConstructShape
    {
        // Can only be removed from an active shape
        Assert.IsTrue(activeShapes.Contains(shape));
        activeShapes.Remove(shape);
        OnShapeParticipationEvent(this, EventType.Remove, shape);

        // If we are dependant on this shape then expect to be removed from the construct
        // The construct should pick this up in the OnActiveShapeEvent
        if (shape == constructionDependantShape)
        {
            Assert.IsNull(construct);
            constructionDependantShape = null;
        }
    }

    public Vector3 GetCentre()
    {
        return worldObject.transform.position;
    }

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
        weightClass = GetWeightClass(worldObject.Weight);
        ConstructPart.GlobalParts.Add(this);
    }

    private void OnDestroy()
    {
        ConstructPart.GlobalParts.Remove(this);
    }

    private void ReleaseControl()
    {
        Assert.IsTrue(IsControlled);
        controller = null;
    }

    private void AddMovement(ConstructMovement movement) // Expects caller to be self
    {
        Assert.IsFalse(movements.Contains(movement));
        movements.Add(movement);
        OnMovementEvent(this, EventType.Add, movement);
    }

    private void RemoveMovement(ConstructMovement movement) // Expects caller to be self
    {
        Assert.IsTrue(movements.Contains(movement));
        movements.Remove(movement);
        OnMovementEvent(this, EventType.Remove, movement);
    }

    private void AddSkill(ConstructSkill skill) // Expects caller to be self
    {
        Assert.IsFalse(skills.Contains(skill));
        skills.Add(skill);
        OnSkillEvent(this, EventType.Add, skill);
    }

    private void RemoveSkill(ConstructSkill skill) // Expects caller to be self
    {
        Assert.IsTrue(skills.Contains(skill));
        skills.Remove(skill);
        OnSkillEvent(this, EventType.Remove, skill);
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
            Part.worldObject.RB.linearDamping = drag;
            Part.worldObject.RB.angularDamping = angularDrag;
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
