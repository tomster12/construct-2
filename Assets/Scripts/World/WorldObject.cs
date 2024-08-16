using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
    public Rigidbody RB => GetCachedComponent<Rigidbody>();
    public Collider Collider => GetCachedComponent<Collider>();
    public Bounds Bounds => Collider.bounds;
    public Vector3 Extents => Bounds.extents;
    public float MaxExtent => Mathf.Max(Bounds.extents.x, Bounds.extents.y, Bounds.extents.z);
    public float MaxExtentXZ => Mathf.Max(Bounds.extents.x, Bounds.extents.z);
    public float Weight { get; set; } = 0f;

    public void InitPhysical()
    {
        Weight = RB.mass;
    }

    public T GetCachedComponent<T>() where T : Component
    {
        Type type = typeof(T);
        if (!componentCache.ContainsKey(type))
        {
            componentCache[type] = GetComponent<T>();
        }
        return (T)componentCache[type];
    }

    private Dictionary<Type, Component> componentCache = new Dictionary<Type, Component>();
}
