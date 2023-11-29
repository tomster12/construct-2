using UnityEngine;

public abstract class ConstructShape : MonoBehaviour, IPartController
{
    public abstract bool IsControlling { get; protected set; }
    public abstract bool IsBlocking { get; protected set; }
}
