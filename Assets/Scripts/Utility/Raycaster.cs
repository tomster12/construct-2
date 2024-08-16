using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Raycaster
{
    public UnityAction OnTargetChange = delegate { };
    public bool Hit { get; private set; }
    public Vector3 HitPoint { get; private set; }
    public Transform HitTransform { get; private set; }
    public WorldObject HitWorldObject { get; private set; }
    public ConstructPart HitConstructPart => HitWorldObject?.GetCachedComponent<ConstructPart>();

    public Raycaster(Camera camera)
    {
        this.camera = camera;
    }

    public void Update()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, MAX_DISTANCE))
        {
            HitPoint = hit.point;
            Hit = true;
            if (hit.collider.transform == HitTransform) return;
            HitTransform = hit.collider.transform;
            HitWorldObject = HitTransform.GetComponent<WorldObject>();
            OnTargetChange.Invoke();
        }
        else
        {
            HitPoint = ray.GetPoint(MAX_DISTANCE);
            Hit = false;
            if (HitTransform == null) return;
            HitTransform = null;
            HitWorldObject = null;
            OnTargetChange.Invoke();
        }
    }

    private Camera camera;

    private static float MAX_DISTANCE = 100f;
}
