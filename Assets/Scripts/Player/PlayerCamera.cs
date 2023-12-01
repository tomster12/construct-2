using Unity.VisualScripting;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform targetCameraParent;
    [SerializeField] private Camera targetCamera;

    [Header("Config")]
    [SerializeField] private float zoomAcceleration = 600.0f;
    [SerializeField] private float zoomDrag = 0.9f;
    [SerializeField] private float zoomVelocityMax = 6.0f;
    [SerializeField] private Vector3 boundsOffsetMult = new Vector3(1.0f, 0.5f, -1.0f);
    [SerializeField] private Vector3 additionalOffset = new Vector3(1.0f, 0.0f, 0.0f);

    public WorldObject TargetWO { get; private set; }
    public Transform TargetCameraParent => targetCameraParent;
    public Camera TargetCamera => targetCamera;

    private Vector3 boundsOffset;
    private Vector3 zoomOffset;
    private float zoomVelocity = 0.0f;
    private float zoomDistance = 5.0f;


    private void Awake()
    {
        TargetCamera.transform.parent = TargetCameraParent;
        LockMouse();
    }

    private void Update()
    {
        if (TargetWO == null) return;

        HandleInput();

    }

    private void FixedUpdate()
    {
        if (TargetWO == null) return;

        FixedUpdateZoom();
        FixedUpdatePosition();
    }

    private void HandleInput()
    {
        // Update zoom velocity
        zoomVelocity += -Input.mouseScrollDelta.y * zoomAcceleration * Time.deltaTime;

        // Update aiming

        TargetCameraParent.transform.RotateAround(TargetCameraParent.transform.position, Vector3.up, Input.GetAxisRaw("Mouse X"));
        TargetCameraParent.transform.RotateAround(TargetCameraParent.transform.position, TargetCameraParent.transform.right, -Input.GetAxisRaw("Mouse Y"));
    }

    private void FixedUpdateZoom()
    {
        // Update zoom dynamics
        zoomVelocity = Mathf.Clamp(zoomVelocity, -zoomVelocityMax, zoomVelocityMax);
        zoomDistance += zoomVelocity * Time.deltaTime;
        zoomVelocity *= zoomDrag;

        // Update zoom offset
        zoomOffset = zoomDistance * Vector3.back;
    }

    private void FixedUpdatePosition()
    {
        TargetCameraParent.transform.position = TargetWO.transform.position;
        TargetCamera.transform.localPosition = boundsOffset + zoomOffset;
    }

    private void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetTarget(WorldObject targetWO)
    {
        TargetWO = targetWO;
        boundsOffset = targetWO.XYMaxExtent * boundsOffsetMult + additionalOffset;
        zoomDistance = targetWO.XYMaxExtent * 15.0f;
        FixedUpdateZoom();
        FixedUpdatePosition();
    }
}
