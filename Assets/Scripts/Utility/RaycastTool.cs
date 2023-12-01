using UnityEngine;

class Raycaster
{
    private static float MAX_DISTANCE = 100f;

    public bool Hit { get; private set; }
    public Transform HitTransform { get; private set; }
    public Vector3 HitPoint { get; private set; }

    public void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, MAX_DISTANCE))
        {
            HitTransform = hit.collider.transform;
            HitPoint = hit.point;
            Hit = true;
        }
        else
        {
            HitTransform = null;
            HitPoint = ray.GetPoint(MAX_DISTANCE);
            Hit = false;
        }
    }
}
