using System;
using UnityEngine;

public enum ActionUseType { SINGLE, CONTINUOUS, CHANNELED }

public abstract class ConstructAction : MonoBehaviour
{
    public abstract string ActionName { get; }
    public abstract ActionUseType UseType { get; }
    public virtual bool CanUse => !IsActive && !IsCooldown;
    public abstract bool IsActive { get; }
    public abstract bool IsCooldown { get; }

    public abstract void UseDown();
    public abstract void UseUp();
    public abstract void Visualise();
}
