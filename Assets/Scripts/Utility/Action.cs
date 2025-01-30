using System;
using UnityEngine;
using UnityEngine.Assertions;

public enum ActionUseType
{ SINGLE, TOGGLE, CHANNELED }

public abstract class Action : MonoBehaviour
{
    public bool IsAssigned => actionSet != null;
    public virtual bool CanUse => !IsActive && !IsCooldown;

    public abstract string ActionName { get; }
    public abstract ActionUseType UseType { get; }
    public abstract bool IsActive { get; }
    public abstract bool IsCooldown { get; }

    protected ActionSet actionSet;

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
}
