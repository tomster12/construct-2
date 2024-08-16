using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

// Attacher: Small, Sharp, Not In Shape
// Attachee: Large, Not In Shape
// Slot 0: Attachee, (locked)
// Slot 1: Attacher
public class AttachmentShape : ConstructShape
{
    public override (bool, int) CanConstructWith(ConstructPart part)
    {
        bool canConstruct = true;
        canConstruct &= !IsConstructing && !IsConstructed;
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

    public override IEnumerator EnumConstructWith(ConstructPart part, int slot, Action<bool> callback)
    {
        var canConstructWith = CanConstructWith(part);
        Assert.IsTrue(canConstructWith.Item1);
        Assert.IsTrue(slot == 1 && canConstructWith.Item2 == slot);

        IsConstructing = true;

        // Perform attachment movement
        IAttacherMovement attacherMovement = part.CurrentController as IAttacherMovement;
        yield return attacherMovement.EnumStartAttach(attacheePart, (bool success) =>
        {
            // TODO: Implement cancelling
            Assert.IsTrue(success);

            // Stop movement control
            ConstructMovement movement = part.CurrentController as ConstructMovement;
            movement.Deactivate();

            // Add attacher part into this shape
            attachingPart = part;
            Parts.Add(attachingPart);
            attachingPart.JoinShape(this);

            // Start shape control
            attachingPartPC = attachingPart.TakeControl(this);
            attachingPartPC.SetPhysicsMode(true, false);
            attachingPartPC.SetEnableCollisions(false);

            IsConstructing = false;
            OnPartsChange.Invoke();
            callback(true);
        });
    }

    [Header("References")]
    [SerializeField] private ConstructPart attacheePart;

    private ConstructPart attachingPart;
    private ConstructPart.PhysicalHandle attachingPartPC;

    private void Awake()
    {
        Assert.IsTrue(attacheePart != null);
        Parts.Add(attacheePart);
        OnPartsChange.Invoke();
    }
}
