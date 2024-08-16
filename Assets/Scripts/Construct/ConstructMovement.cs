using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ConstructMovement : MonoBehaviour, IPartController
{
    public UnityAction<bool> OnStateChange { get; set; } = delegate { };
    public bool IsActive { get; protected set; }

    public abstract void Move(Vector3 dir);

    public abstract void Aim(Vector3 pos);

    public abstract bool CanActivate();

    public abstract void Activate();

    public abstract void Deactivate();

    public abstract Vector3 GetCentre();
}
