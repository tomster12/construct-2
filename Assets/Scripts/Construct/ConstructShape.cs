using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public abstract class ConstructShape : MonoBehaviour, IPartController
{
    public enum TransitionType
    { None, Constructing, Adding, Removing, Deconstructing }

    public UnityAction<ConstructShape, bool> OnPartEvent { get; set; } = delegate { };
    public List<ConstructPart> Parts { get; } = new();
    public bool IsConstructed { get; protected set; }
    public TransitionType transitionType { get; protected set; }

    public abstract (bool, int) CanAddPart(ConstructPart part);

    public abstract Task<Boolean> AddPart(ConstructPart part, int slot);

    public abstract Task RemovePart(ConstructPart part);

    public abstract Task Deconstruct();

    public virtual int GetSlotCount() => 0;

    public virtual ConstructPart GetSlot(int slot) => null;
}
