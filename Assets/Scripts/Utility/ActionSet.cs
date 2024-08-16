using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class ActionSet
{
    public ActionSet(int slotCount = 5)
    {
        this.slotCount = slotCount;
        this.actions = new Action[slotCount];
        this.availableSlots = new HashSet<int>();
        for (int i = 0; i < slotCount; i++) this.availableSlots.Add(i);
    }

    public int AvailableSlotCount => availableSlots.Count;

    public Action ActionInputDown(int slot)
    {
        if (slot < 0 || slot >= this.slotCount) return null;
        if (actions[slot] == null) return null;
        actions[slot].InputDown();
        return actions[slot];
    }

    public Action ActionInputUp(int slot)
    {
        if (slot < 0 || slot >= this.slotCount) return null;
        if (actions[slot] == null) return null;
        actions[slot].InputUp();
        return actions[slot];
    }

    public bool RegisterAction(Action action, int slot = -1)
    {
        Assert.IsFalse(action.IsAssigned);

        if (slot != -1)
        {
            if (slot < 0 || slot >= slotCount) throw new ArgumentOutOfRangeException();
            Assert.IsNull(actions[slot]);
            actions[slot] = action;
        }
        else
        {
            Assert.IsFalse(AvailableSlotCount == 0);
            slot = availableSlots.Min();
            availableSlots.Remove(slot);
            actions[slot] = action;
        }

        action.Assign(this);
        return true;
    }

    public bool UnregisterAction(Action action)
    {
        Assert.IsTrue(action.IsAssigned);
        int slot = Array.IndexOf(actions, action);
        Assert.IsTrue(slot != -1);
        action.Unnassign();
        actions[slot] = null;
        availableSlots.Add(slot);
        return true;
    }

    public bool UnregisterAction(int slot)
    {
        if (slot < 0 || slot >= this.slotCount) throw new ArgumentOutOfRangeException();
        Assert.IsNotNull(actions[slot]);
        actions[slot].Unnassign();
        actions[slot] = null;
        availableSlots.Add(slot);
        return true;
    }

    private Action[] actions;
    private HashSet<int> availableSlots;
    private int slotCount;
}
