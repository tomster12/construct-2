using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

// Attacher: Small, Sharp, Not In Shape
// Attachee: Large, Not In Shape
// Slot 0: Attachee, (locked)
// Slot 1: Attacher
public class AttachmentShape : ConstructShape
{
    [Header("References")]
    [SerializeField] private ConstructPart attacheePart;

    private ConstructPart attachingPart;
    private ConstructPart.PhysicalHandle attachingPartPH;

    public override (bool, int) CanAddPart(ConstructPart part)
    {
        bool canConstruct = true;
        canConstruct &= (transitionType == TransitionType.None) && !IsConstructed;
        canConstruct &= attacheePart != null;
        canConstruct &= attachingPart == null;
        canConstruct &= part.Tags.Contains(PartTag.Sharp);
        canConstruct &= part.WeightClass == PartWeightClass.S;
        canConstruct &= part.IsControlled && part.CurrentController is ConstructMovement && part.CurrentController is IAttacherMovement;

        return (canConstruct, 1);
    }

    public override int GetSlotCount() => 2;

    public override ConstructPart GetSlot(int slot)
    {
        switch (slot)
        {
            case 0: return attacheePart;
            case 1: return attachingPart;
            default: return null;
        }
    }

    public override async Task<bool> AddPart(ConstructPart part, int slot)
    {
        var canConstructWith = CanAddPart(part);
        Assert.IsTrue(canConstructWith.Item1);
        Assert.IsTrue(slot == 1 && canConstructWith.Item2 == slot);

        transitionType = TransitionType.Constructing;

        IAttacherMovement attacherMovement = part.CurrentController as IAttacherMovement;
        bool success = await attacherMovement.AttachTo(attacheePart);
        Assert.IsTrue(success);

        ConstructMovement movement = part.CurrentController as ConstructMovement;
        movement.Deactivate();

        attachingPart = part;
        Parts.Add(attachingPart);

        attachingPart.NotifyAddedToActiveShape(this);
        attacheePart.NotifyAddedToActiveShape(this);

        attachingPartPH = attachingPart.TakeControl(this);
        attachingPartPH.SetPhysicsMode(true, false);
        attachingPartPH.SetEnableCollisions(false);

        transitionType = TransitionType.None;
        IsConstructed = true;

        OnPartsChange.Invoke(this, true);
        Assert.IsTrue(attacheePart.IsConstructed);
        Assert.IsTrue(attachingPart.IsConstructed);
        return true;
    }

    public override async Task RemovePart(ConstructPart part)
    {
        // Only should ever be allowed to remove the attaching part
        // This is equivalent to deconstruction so redirect to that function
        Assert.IsTrue(IsConstructed);
        Assert.IsTrue(transitionType == TransitionType.None);
        Assert.IsTrue(attachingPart == part);
        await Deconstruct();
    }

    public override Task Deconstruct()
    {
        Assert.IsTrue(IsConstructed);
        Assert.IsTrue(transitionType == TransitionType.None);

        transitionType = TransitionType.Deconstructing;

        attachingPartPH.Release();

        // Unparent attaching shape
        // In the future this should be more complex
        attachingPart.WO.transform.SetParent(null);

        Parts.Remove(attachingPart);
        attachingPart.NotifyRemovedFromActiveShape(this);
        attachingPart = null;

        attacheePart.NotifyRemovedFromActiveShape(this);

        transitionType = TransitionType.None;
        IsConstructed = false;

        OnPartsChange.Invoke(this, false);
        Assert.IsFalse(attacheePart.IsConstructed);
        Assert.IsFalse(attachingPart.IsConstructed);

        // Currently not async so return task result
        return Task.CompletedTask;
    }

    private void Awake()
    {
        Assert.IsTrue(attacheePart != null);
        Parts.Add(attacheePart);
        OnPartsChange.Invoke(this, true);
    }
}
