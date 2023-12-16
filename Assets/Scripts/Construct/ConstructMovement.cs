using UnityEngine;

public abstract class ConstructMovement : MonoBehaviour, IPartController
{
    public virtual bool isControlled { get; protected set; }
    public abstract bool IsBlocking { get; }

    public abstract void Move(Vector3 dir);
    public abstract void Aim(Vector3 pos);
    public abstract void SetControlled(bool isControlled);
    public abstract bool CanSetControlled(bool isControlled);
}
