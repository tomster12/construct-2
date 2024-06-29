using UnityEngine;
using UnityEngine.Events;

public abstract class ConstructMovement : MonoBehaviour, IPartController
{
    public UnityAction OnChangeControlling = delegate { };
    public abstract bool IsControlling { get; protected set; }

    public abstract void Move(Vector3 dir);

    public abstract void Aim(Vector3 pos);

    public abstract void SetControlling();

    public abstract void UnsetControlling();

    public abstract bool CanSetControlling();

    public abstract bool CanUnsetControlling();

    public abstract bool CanEnterForging();

    public abstract bool CanExitForging();

    public abstract Vector3 GetCentre();
}
