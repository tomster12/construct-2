using System;
using System.Collections.Generic;
using System.Linq;

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
        if (action.IsAssigned) throw new Exception("RegisterAction(action): already assigned.");
        if (slot != -1)
        {
            if (slot < 0 || slot >= this.slotCount) throw new ArgumentOutOfRangeException();
            if (actions[slot] != null) throw new Exception("RegisterAction(action): slot already assigned.");
            actions[slot] = action;
        }
        else
        {
            if (AvailableSlotCount == 0) throw new Exception("!RegisterAction(action): no available slots.");
            slot = availableSlots.Min();
            availableSlots.Remove(slot);
            actions[slot] = action;
        }
        action.Assign(this);
        return true;
    }

    public bool UnregisterAction(Action action)
    {
        if (!action.IsAssigned) throw new Exception("Cannot UnregisterAction(action) not assigned!");
        int slot = Array.IndexOf(actions, action);
        if (slot == -1) throw new Exception("Cannot UnregisterAction(action) not found!");
        action.Unnassign();
        actions[slot] = null;
        availableSlots.Add(slot);
        return true;
    }

    public bool UnregisterAction(int slot)
    {
        if (slot < 0 || slot >= this.slotCount) throw new ArgumentOutOfRangeException();
        if (actions[slot] == null) throw new Exception("Cannot UnregisterAction(slot) no action in slot!");
        actions[slot].Unnassign();
        actions[slot] = null;
        availableSlots.Add(slot);
        return true;
    }

    private Action[] actions;
    private HashSet<int> availableSlots;
    private int slotCount;
}
