using UnityEngine;
using UnityEngine.Assertions;

public class AttachmentShape : ConstructShape
{
    // Attacher: Small, Sharp, Not In Shape
    // Attachee: Large, Not In Shape
    // Slot 0: Attachee, (locked)
    // Slot 1: Attacher

    public override bool IsConstructed => isConstructed;
    public ConstructPart AttacheePart { get; private set; } = null;
    public ConstructPart AttacherPart { get; private set; } = null;

    public override bool CanEnterForging() => false;

    public override bool CanExitForging() => true;

    public override (bool, int) CanConstructWith(ConstructPart part)
    {
        bool canConstruct = true;
        canConstruct &= AttacheePart != null;
        canConstruct &= AttacherPart == null;
        canConstruct &= part.Tags.Contains(ConstructPart.TagType.Sharp);
        canConstruct &= part.WeightClass == ConstructPart.WeightClassType.S;
        canConstruct &= part.IsConstructed && part.CurrentController is IAttacherMovement;
        return (canConstruct, 1);
    }

    public override void ConstructWith(ConstructPart part, int slot)
    {
        if (!CanConstructWith(part).Item1) throw new System.Exception("Cannot ConstructWith(part)!");
        Assert.IsTrue(slot == 1);

        AttacherPart = part;
        Parts.Add(AttacherPart);
        AttacherPart.OnJoinShape(this);

        // Move control to this shape
        ConstructMovement movement = AttacherPart.CurrentController as ConstructMovement;
        movement.UnsetControlling();
        AttacherPart.SetController(this);

        isConstructed = true;
    }

    [Header("References")]
    [SerializeField] private ConstructPart initAttacheePart;

    private bool isConstructed = false;

    private void Awake()
    {
        Assert.IsTrue(initAttacheePart != null);
        AttacheePart = initAttacheePart;
        Parts.Add(AttacheePart);
    }
}
