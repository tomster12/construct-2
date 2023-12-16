using UnityEngine;

public class AttacheeComponent : PartComponent
{
    public ConstructPart AttacheePart { get; private set; }

    private void Awake()
    {
        AttacheePart = GetComponent<ConstructPart>();
    }
}