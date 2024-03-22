using System;
using UnityEngine;
using UnityEngine.Assertions;

public enum ActionUseType
{ SINGLE, CONTINUOUS, CHANNELED }

public abstract class ConstructAction : MonoBehaviour
{
    public abstract string ActionName { get; }
    public abstract ActionUseType UseType { get; }
    public bool IsAssigned => actionSet != null;
    public abstract bool IsActive { get; }
    public abstract bool IsCooldown { get; }
    public virtual bool CanUse => !IsActive && !IsCooldown;

    public abstract void InputDown();

    public abstract void InputUp();

    public void SetActionSet(ConstructActionSet actionSet)
    {
        if (this.actionSet != null) throw new Exception("Cannot SetActionSet(actionSet), already assigned!");
        this.actionSet = actionSet;
    }

    protected ConstructActionSet actionSet;
}
