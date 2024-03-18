using UnityEngine;

internal class Raycaster
{
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
            HitTransform = hit.collider.transform;
            HitPoint = hit.point;
            HitWorldObject = HitTransform.GetComponent<WorldObject>();
            Hit = true;
        }
        else
        {
            HitTransform = null;
            HitPoint = ray.GetPoint(MAX_DISTANCE);
            HitWorldObject = null;
            Hit = false;
        }
    }

    private Camera camera;

    private static float MAX_DISTANCE = 100f;
}
