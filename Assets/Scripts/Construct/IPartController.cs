public interface IPartController
{
    public bool IsControlling { get; }
    public bool CanTransition { get; }

    public void SetControlling();

    public void UnsetControlling();

    public bool CanSetControlling();

    public bool CanUnsetControlling();
}
