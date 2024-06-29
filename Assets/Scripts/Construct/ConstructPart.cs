using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(WorldObject))]
public class ConstructPart : MonoBehaviour
{
    public static List<ConstructPart> Parts = new List<ConstructPart>();

    public enum TagType
    { Core, Sharp };

    public enum WeightClassType
    { S, M, L, XL };

    public UnityAction OnPropertiesChange = delegate { };
    public WorldObject BaseWO => baseWO;
    public Sprite Icon => icon;
    public List<TagType> Tags => tags;
    public List<ConstructShape> Shapes => shapes;
    public List<ConstructShape> ActiveShapes => shapes.FindAll(shape => shape.IsConstructed);
    public WeightClassType WeightClass { get; private set; } = WeightClassType.S;
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

    public void SetController(IPartController controller)
    {
        if (controller == null) throw new Exception("Cannot SetController(null).");
        if (!CanControl) throw new Exception("Cannot SetController(controller) when !CanControl.");
        CurrentController = controller;
    }

    public void UnsetController()
    {
        if (!IsControlled) throw new Exception("Cannot UnsetController() when not controlled.");
        CurrentController = null;
    }

    public void RegisterMovement(ConstructMovement movement)
    {
        if (movements.Contains(movement)) throw new Exception("Cannot RegisterMovement(movement), already registered!");
        movements.Add(movement);
        if (IsConstructed) CurrentConstruct.OnRegisterMovement(movement);
        OnPropertiesChange();
    }

    public void UnregisterMovement(ConstructMovement movement)
    {
        if (!movements.Contains(movement)) throw new Exception("Cannot UnregisterMovement(movement), not registered!");
        movements.Remove(movement);
        if (IsConstructed) CurrentConstruct.OnUnregisterMovement(movement);
        OnPropertiesChange();
    }

    public void RegisterSkill(ConstructSkill skill)
    {
        if (skills.Contains(skill)) throw new Exception("Cannot RegisterSkill(skill), already registered!");
        skills.Add(skill);
        if (IsConstructed) CurrentConstruct.OnRegisterSkill(skill);
        OnPropertiesChange();
    }

    public void UnregisterSkill(ConstructSkill skill)
    {
        if (!skills.Contains(skill)) throw new Exception("Cannot UnregisterSkill(skill), not registered!");
        skills.Remove(skill);
        if (IsConstructed) CurrentConstruct.OnUnregisterSkill(skill);
    }

    public Vector3 GetCentre()
    {
        return baseWO.transform.position;
    }

    public void OnJoinConstruct(Construct construct)
    {
        if (CurrentConstruct != null) throw new Exception("Cannot OnJoinConstruct(construct) when already joined to construct.");
        CurrentConstruct = construct;
        foreach (ConstructMovement movement in movements) construct.OnRegisterMovement(movement);
        foreach (ConstructSkill skill in skills) construct.OnRegisterSkill(skill);
        foreach (ConstructShape shape in shapes) construct.OnRegisterShape(shape);
    }

    public void OnLeaveConstruct(Construct construct)
    {
        if (CurrentConstruct != construct) throw new Exception("Cannot OnleaveConstruct(construct) when not joined to construct.");
        foreach (ConstructMovement movement in movements) construct.OnUnregisterMovement(movement);
        foreach (ConstructSkill skill in skills) construct.OnUnregisterSkill(skill);
        foreach (ConstructShape shape in shapes) construct.OnUnregisterShape(shape);
        CurrentConstruct = null;
    }

    public void OnJoinShape(ConstructShape shape)
    {
        if (shapes.Contains(shape)) throw new Exception("Cannot RegisterShape(shape), already registered!");
        shapes.Add(shape);
        if (IsConstructed) CurrentConstruct.OnRegisterShape(shape);
        OnPropertiesChange();
    }

    public void OnLeaveShape(ConstructShape shape)
    {
        if (!shapes.Contains(shape)) throw new Exception("Cannot UnregisterShape(shape), not registered!");
        shapes.Remove(shape);
        if (IsConstructed) CurrentConstruct.OnUnregisterShape(shape);
        OnPropertiesChange();
    }

    [SerializeField] private WorldObject baseWO;
    [SerializeField] private Sprite icon;
    [SerializeField] private List<TagType> tags = new();
    [SerializeField] private List<ConstructMovement> movements = new();
    [SerializeField] private List<ConstructSkill> skills = new();
    [SerializeField] private List<ConstructShape> shapes = new();

    private static WeightClassType GetWeightClass(float weight)
    {
        if (weight <= 5f) return WeightClassType.S;
        if (weight <= 25f) return WeightClassType.M;
        return WeightClassType.L;
    }

    private void Awake()
    {
        // Initialize physical properties
        baseWO.InitPhysical();
        WeightClass = GetWeightClass(baseWO.Weight);
        ConstructPart.Parts.Add(this);
    }

    public void OnDestroy()
    {
        ConstructPart.Parts.Remove(this);
    }

    private void OnDrawGizmos()
    {
        // TODO: Draw indicators for constructed / controlled / controlled type
    }
}
