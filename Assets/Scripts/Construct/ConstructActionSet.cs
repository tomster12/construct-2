using System;
using System.Collections.Generic;

public class ConstructActionSet
{
    public ConstructActionSet(Construct construct)
    {
        Construct = construct;
    }

    public Construct Construct { get; private set; }

    public ConstructAction ActionInputDown(string actionName)
    {
        if (actions.ContainsKey(actionName))
        {
            actions[actionName].InputDown();
            return actions[actionName];
        }
        return null;
    }

    public ConstructAction ActionInputUp(string actionName)
    {
        if (actions.ContainsKey(actionName))
        {
            actions[actionName].InputUp();
            return actions[actionName];
        }
        return null;
    }

    public bool RegisterAction(ConstructAction action)
    {
        if (action.IsAssigned) return Utility.LogWarning("Cannot RegisterAction(action) already assigned!");
        if (actions.ContainsKey(action.ActionName)) return Utility.LogWarning("Cannot RegisterAction(action), actionName already registered!");
        actions.Add(action.ActionName, action);
        action.SetActionSet(this);
        return true;
    }

    public bool DeregisterAction(ConstructAction action)
    {
        if (!action.IsAssigned) return Utility.LogWarning("Cannot DeregisterAction(action) not assigned!");
        if (!actions.ContainsKey(action.ActionName)) return Utility.LogWarning("Cannot DeregisterAction(action) not registered!");
        actions.Remove(action.ActionName);
        action.SetActionSet(null);
        return true;
    }

    private Dictionary<string, ConstructAction> actions = new Dictionary<string, ConstructAction>();
}
