using UnityEngine;

public abstract class ConstructMovement : MonoBehaviour, IPartController
{
    public abstract bool IsControlling { get; protected set; }

    public abstract void Move(Vector3 dir);
    public abstract void Aim(Vector3 pos);
    public abstract void SetControlling(bool isControlling);
}
