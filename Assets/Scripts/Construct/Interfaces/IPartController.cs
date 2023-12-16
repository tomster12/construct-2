public interface IPartController
{
    public abstract bool isControlled { get; }
    public abstract bool IsBlocking { get; }

    public abstract void SetControlled(bool isControlled);
    public abstract bool CanSetControlled(bool isControlled);
}
