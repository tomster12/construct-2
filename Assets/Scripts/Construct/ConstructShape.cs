using UnityEngine;

public abstract class ConstructShape : MonoBehaviour, IPartController
{
    public virtual bool IsControlling { get; protected set; }
    public abstract bool IsBlocking { get; }

    public abstract void SetControlling();

    public abstract void UnsetControlling();

    public abstract bool CanSetControlling();

    public abstract bool CanUnsetControlling();
}
