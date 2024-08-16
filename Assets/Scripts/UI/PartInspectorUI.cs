using UnityEngine;

public class PartInspectorUI : MonoBehaviour
{
    public void Init(ConstructPart part)
    {
        this.partView.Init(part);
        this.part = part;
    }

    [Header("References")]
    [SerializeField] private ConstructPartUI partView;
    [SerializeField] private Transform billboard;

    private ConstructPart part;

    private void Update()
    {
        if (part == null) return;

        // Face towards camera
        Vector3 target = Camera.main.transform.position;
        billboard.forward = billboard.position - target;

        // Set position relative to part
        billboard.position = part.transform.position + part.transform.up * 0.8f;
    }
}
