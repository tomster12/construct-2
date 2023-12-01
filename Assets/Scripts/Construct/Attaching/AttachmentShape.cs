
using UnityEngine;

public class AttachmentShape : ConstructShape
{
    public override bool IsBlocking => false;

    public ConstructPart AttachingPart { get; private set; }
    public ConstructPart AttacheePart { get; private set; }

    public override void SetControlling(bool isControlling)
    {
        IsControlling = isControlling;
    }

    public void SetParts(ConstructPart attachingPart, ConstructPart attacheePart)
    {
        AttachingPart = attachingPart;
        AttacheePart = attacheePart;
    }

    public override bool CanSetControlling(bool isControlling)
    {
        if (IsBlocking) return false;
        if (isControlling && (IsControlling || (AttachingPart.IsControlled || AttacheePart.IsControlled))) throw new System.Exception("Cannot SetControlling(true) when already controlled.");
        else if (!isControlling && !IsControlling) throw new System.Exception("Cannot SetControlling(false) when not controlled.");
        return true;
    }
}