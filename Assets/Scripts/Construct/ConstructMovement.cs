using UnityEngine;

public abstract class ConstructMovement : MonoBehaviour, IPartController
{
    public virtual bool IsControlling { get; protected set; }
    public abstract bool IsBlocking { get; }

    public abstract void Move(Vector3 dir);

    public abstract void Aim(Vector3 pos);

    public abstract void SetControlling();

    public abstract void UnsetControlling();

    public abstract bool CanSetControlling();

    public abstract bool CanUnsetControlling();
}
