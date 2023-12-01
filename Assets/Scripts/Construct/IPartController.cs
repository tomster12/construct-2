public interface IPartController
{
    public abstract bool IsControlling { get; }
    public abstract bool IsBlocking { get; }

    public abstract void SetControlling(bool isControlling);
    public abstract bool CanSetControlling(bool isControlling);
}
