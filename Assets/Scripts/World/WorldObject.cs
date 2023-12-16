
using UnityEngine;

// TODO: Expand this with component cache.
public class WorldObject : MonoBehaviour
{
    public Bounds Bounds { get; private set; }
    public float MaxExtent => Mathf.Max(Bounds.extents.x, Bounds.extents.y, Bounds.extents.z);
    public float XZMaxExtent => Mathf.Max(Bounds.extents.x, Bounds.extents.z);

    private void Awake()
    {
        Bounds = GetComponent<Collider>().bounds;
    }
}