using UnityEngine;

public abstract class ConstructShape : MonoBehaviour, IPartController
{
    public virtual bool IsControlling { get; protected set; }
    public abstract bool IsBlocking { get; }

    public abstract void SetControlling(bool isControlling);
    public abstract bool CanSetControlling(bool isControlling);
}
