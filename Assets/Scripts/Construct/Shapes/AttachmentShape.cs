public class AttachmentShape : ConstructShape
{
    public override bool IsBlocking => false;

    public ConstructPart AttachingPart { get; private set; }
    public ConstructPart AttacheePart { get; private set; }

    public void SetParts(ConstructPart attachingPart, ConstructPart attacheePart)
    {
        AttachingPart = attachingPart;
        AttacheePart = attacheePart;
    }

    public override void SetControlling()
    {
        if (!CanSetControlling()) throw new System.Exception("Cannot SetControlling(true) when already controlled.");
        IsControlling = true;
        AttachingPart.SetController(this);
        AttacheePart.SetController(this);
        AttachingPart.Construct.AddPart(AttacheePart);
    }

    public override void UnsetControlling()
    {
        if (!CanUnsetControlling()) throw new System.Exception("Cannot UnsetControlling(false) when not controlled.");
        IsControlling = false;
        AttachingPart.UnsetController();
        AttacheePart.UnsetController();
        AttachingPart.Construct.RemovePart(AttacheePart);
    }

    public override bool CanSetControlling()
    {
        if (IsBlocking) return false;
        if (IsControlling || AttachingPart.IsControlled || AttacheePart.IsControlled) throw new System.Exception("Cannot SetControlling(true) when already controlled.");
        return true;
    }

    public override bool CanUnsetControlling()
    {
        if (IsBlocking) return false;
        if (!IsControlling) throw new System.Exception("Cannot UnsetControlling(false) when not controlled.");
        return true;
    }
}
