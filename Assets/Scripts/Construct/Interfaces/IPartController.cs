public interface IPartController
{
    public abstract bool IsControlling { get; }
    public abstract bool IsBlocking { get; }

    public abstract void SetControlling();
    public abstract void UnsetControlling();
    public abstract bool CanSetControlling();
    public abstract bool CanUnsetControlling();
}
