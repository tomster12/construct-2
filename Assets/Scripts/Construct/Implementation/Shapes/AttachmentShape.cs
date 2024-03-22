using UnityEngine;

public class AttachmentShape : MonoBehaviour, IConstructShape
{
    public bool IsControlling { get; private set; }
    public bool CanTransition => false;
    public ConstructPart AttachingPart { get; private set; }
    public ConstructPart AttacheePart { get; private set; }

    public void SetParts(ConstructPart attachingPart, ConstructPart attacheePart)
    {
        if (attachingPart == null) throw new System.ArgumentNullException(nameof(attachingPart));
        if (attacheePart == null) throw new System.ArgumentNullException(nameof(attacheePart));
        if (attachingPart == attacheePart) throw new System.ArgumentException("Cannot attach a part to itself.");
        if (AttachingPart != null) throw new System.InvalidOperationException("Cannot set parts when already set.");
        if (AttacheePart != null) throw new System.InvalidOperationException("Cannot set parts when already set.");
        AttachingPart = attachingPart;
        AttacheePart = attacheePart;
    }

    public void SetControlling()
    {
        if (!CanSetControlling()) throw new System.Exception("Cannot SetControlling(true) when already controlled.");
        IsControlling = true;
        AttachingPart.SetController(this);
        AttacheePart.SetController(this);
        AttachingPart.Construct.AddPart(AttacheePart);
    }

    public void UnsetControlling()
    {
        if (!CanUnsetControlling()) throw new System.Exception("Cannot UnsetControlling(false) when not controlled.");
        IsControlling = false;
        AttachingPart.UnsetController();
        AttacheePart.UnsetController();
        AttachingPart.Construct.RemovePart(AttacheePart);
    }

    public bool CanSetControlling() => CanTransition && IsControlling && AttachingPart.IsControlled && AttacheePart.IsControlled;

    public bool CanUnsetControlling() => CanTransition && !IsControlling;
}
