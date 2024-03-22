using UnityEngine;

public class PartComponent : MonoBehaviour
{
    public ConstructPart Part { get; private set; }

    public virtual void Init(ConstructPart part)
    {
        this.Part = part;
    }
}
