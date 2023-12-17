using System;
using System.Collections.Generic;

public class ConstructActionSet
{
    private Dictionary<string, ConstructAction> actions = new Dictionary<string, ConstructAction>();

    public Construct Construct { get; private set; }

    public ConstructActionSet(Construct construct)
    {
        Construct = construct;
    }

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
        if (action.IsAssigned) throw new Exception("Cannot RegisterAction(action) already assigned!");
        actions.Add(action.ActionName, action);
        action.SetActionSet(this);
        return true;
    }

    public bool DeregisterAction(ConstructAction action)
    {
        if (!action.IsAssigned) throw new Exception("Cannot DeregisterAction(action) not assigned!");
        if (!actions.ContainsKey(action.ActionName)) throw new Exception("Cannot DeregisterAction(action) not registered!");
        actions.Remove(action.ActionName);
        action.SetActionSet(null);
        return true;
    }
}
