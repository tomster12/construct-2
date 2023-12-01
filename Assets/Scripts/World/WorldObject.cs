
using UnityEngine;

public class WorldObject : MonoBehaviour
{
    public Bounds Bounds { get; private set; }
    public float MaxExtent => Mathf.Max(Bounds.extents.x, Bounds.extents.y, Bounds.extents.z);
    public float XYMaxExtent => Mathf.Max(Bounds.extents.x, Bounds.extents.y);

    private void Awake()
    {
        Bounds = GetComponent<Collider>().bounds;
    }
}