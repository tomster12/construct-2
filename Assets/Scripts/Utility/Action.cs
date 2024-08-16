using System;
using UnityEngine;
using UnityEngine.Assertions;

public enum ActionUseType
{ SINGLE, TOGGLE, CHANNELED }

public abstract class Action : MonoBehaviour
{
    public abstract string ActionName { get; }
    public abstract ActionUseType UseType { get; }
    public bool IsAssigned => actionSet != null;
    public abstract bool IsActive { get; }
    public abstract bool IsCooldown { get; }
    public virtual bool CanUse => !IsActive && !IsCooldown;

    public virtual void InputDown()
    { }

    public virtual void InputUp()
    { }

    public void Assign(ActionSet actionSet)
    {
        Assert.IsFalse(IsAssigned);
        Assert.IsNotNull(actionSet);
        this.actionSet = actionSet;
    }

    public void Unnassign()
    {
        Assert.IsTrue(IsAssigned);
        this.actionSet = null;
    }

    protected ActionSet actionSet;
}
