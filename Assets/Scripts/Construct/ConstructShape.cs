using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ConstructShape : MonoBehaviour, IPartController
{
    public UnityAction OnPartsChange { get; set; } = delegate { };
    public List<ConstructPart> Parts { get; } = new();
    public bool IsConstructed { get; protected set; }
    public bool IsConstructing { get; protected set; }

    public abstract (bool, int) CanConstructWith(ConstructPart part);

    public abstract IEnumerator EnumConstructWith(ConstructPart part, int slot, Action<bool> callback);

    public virtual int GetSlotCount() => 0;

    public virtual ConstructPart GetSlot(int slot) => null;
}
