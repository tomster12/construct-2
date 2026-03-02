using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Raycaster
{
    public bool HasHit { get; private set; }
    public Vector3 HitPoint { get; private set; }
    public Transform HitTransform { get; private set; }
    public WorldObject HitWorldObject { get; private set; }
    public ConstructPart HitConstructPart => HitWorldObject == null ? null : HitWorldObject.GetCachedComponent<ConstructPart>();
    public UnityAction OnTargetChange = delegate { };

    private static float MAX_DISTANCE = 100f;
    private Camera camera;
    private Ray ray;

    public Raycaster(Camera camera)
    {
        this.camera = camera;
    }

    public void Update()
    {
        ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, MAX_DISTANCE))
        {
            HitPoint = hit.point;
            HasHit = true;
            if (hit.collider.transform == HitTransform) return;

            HitTransform = hit.collider.transform;
            HitWorldObject = HitTransform.GetComponent<WorldObject>();
            OnTargetChange.Invoke();
        }
        else
        {
            HitPoint = ray.GetPoint(MAX_DISTANCE);
            HasHit = false;
            if (HitTransform == null) return;

            HitTransform = null;
            HitWorldObject = null;
            OnTargetChange.Invoke();
        }
    }
}
