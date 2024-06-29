using System;
using UnityEngine;

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
        if (IsAssigned) throw new Exception("Cannot Assign(actionSet), already assigned!");
        if (actionSet == null) throw new Exception("Cannot Assign(actionSet), null!");
        this.actionSet = actionSet;
    }

    public void Unnassign()
    {
        if (!IsAssigned) throw new Exception("Cannot Unnasign(), not assigned!");
        this.actionSet = null;
    }

    protected ActionSet actionSet;
}
