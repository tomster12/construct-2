
using UnityEngine;

public class AttachmentShape : ConstructShape
{
    public override bool IsBlocking => false;

    public ConstructPart AttachingPart { get; private set; }
    public ConstructPart AttacheePart { get; private set; }

    public override void SetControlled(bool isControlled)
    {
        isControlled = isControlled;
    }

    public void SetParts(ConstructPart attachingPart, ConstructPart attacheePart)
    {
        AttachingPart = attachingPart;
        AttacheePart = attacheePart;
    }

    public override bool CanSetControlled(bool isControlled)
    {
        if (IsBlocking) return false;
        if (isControlled && (isControlled || (AttachingPart.IsControlled || AttacheePart.IsControlled))) throw new System.Exception("Cannot SetControlling(true) when already controlled.");
        else if (!isControlled && !isControlled) throw new System.Exception("Cannot SetControlling(false) when not controlled.");
        return true;
    }
}