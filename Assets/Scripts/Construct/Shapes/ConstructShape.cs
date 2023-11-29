using UnityEngine;

public abstract class ConstructShape : MonoBehaviour, IPartController
{
    public abstract bool IsControlling { get; }
}
