using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ConstructShape : MonoBehaviour, IPartController
{
    public UnityAction OnChange = delegate { };
    public List<ConstructPart> Parts { get; private set; } = new();
    public abstract bool IsConstructed { get; }

    public abstract bool CanEnterForging();

    public abstract bool CanExitForging();

    public abstract (bool, int) CanConstructWith(ConstructPart part);

    public abstract void ConstructWith(ConstructPart part, int slot);
}
