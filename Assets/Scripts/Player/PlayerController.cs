using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static readonly Dictionary<PlayerInput, string> ACTION_INPUTS = new Dictionary<PlayerInput, string>()
    {
        { PlayerInput.MouseInput(0), "Attach" }
    };

    [SerializeField] private Transform camParent;
    [SerializeField] private Camera cam;
    [SerializeField] Construct construct;
    [SerializeField] ConstructPart corePart;

    [Header("Config")]
    [SerializeField] private float constructMoveSpeed = 10.0f;
    [SerializeField] private float camZoomAcc = 600.0f;
    [SerializeField] private float camZoomDrag = 0.9f;
    [SerializeField] private float camZoomVelMax = 6.0f;
    [SerializeField] private Vector3 camOffsetBoundsMult = new Vector3(1.0f, 0.5f, -1.0f);
    [SerializeField] private Vector3 camOffsetAdditional = new Vector3(1.0f, 0.0f, 0.0f);
    [SerializeField] private float camAimSpeed = 40.0f;

    private Raycaster raycaster = new Raycaster();
    private Vector3 movementInput;
    private Vector3 aimInput;
    private WorldObject camTarget;
    private Vector3 camOffsetBounds;
    private Vector3 camOffsetZoom;
    private float camZoomVel = 0.0f;
    private float camZoomDistance = 5.0f;

    private void Start()
    {
        cam.transform.parent = camParent;
        construct.SetAndAddCore(corePart);
        SetTarget(corePart.WO);
        LockMouse();
    }

    private void Update()
    {
        HandleInput();
        HandleActions();
        UpdateCam();
    }

    private void HandleInput()
    {
        // Update input direction
        movementInput = Vector3.zero;
        Vector3 flatForward = Vector3.ProjectOnPlane(camParent.forward, Vector3.up).normalized;
        movementInput += camParent.right * Input.GetAxisRaw("Horizontal");
        movementInput += flatForward * Input.GetAxisRaw("Vertical");

        // Update zoom velocity
        camZoomVel += -Input.mouseScrollDelta.y * camZoomAcc * Time.deltaTime;

        // Update aiming
        aimInput = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"), 0.0f);
    }

    private void HandleActions()
    {
        // Check input for each action
        foreach (KeyValuePair<PlayerInput, string> actionInput in ACTION_INPUTS)
        {
            if (actionInput.Key.GetDown())
            {
                construct.UseActionDown(actionInput.Value);
            }
            else if (actionInput.Key.GetUp())
            {
                construct.UseActionUp(actionInput.Value);
            }
        }
    }

    private void UpdateCam()
    {
        // Update cam zoom
        camZoomVel = Mathf.Clamp(camZoomVel, -camZoomVelMax, camZoomVelMax);
        camZoomDistance += camZoomVel * Time.deltaTime;
        camZoomVel *= camZoomDrag;
        camOffsetZoom = camZoomDistance * Vector3.back;

        // Update cam rotation
        camParent.transform.RotateAround(camParent.transform.position, Vector3.up, aimInput.x * Time.deltaTime * camAimSpeed);
        camParent.transform.RotateAround(camParent.transform.position, camParent.transform.right, -aimInput.y * Time.deltaTime * camAimSpeed);

        // Update cam position
        camParent.transform.position = camTarget.transform.position;
        cam.transform.localPosition = camOffsetBounds + camOffsetZoom;

        // Update raycaster
        raycaster.Update();
    }

    private void FixedUpdate()
    {
        FixedHandleMovement();
    }

    private void FixedHandleMovement()
    {
        construct.Move(movementInput * Time.fixedDeltaTime * constructMoveSpeed);
        construct.Aim(raycaster.HitPoint);
    }

    private void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetTarget(WorldObject targetWO)
    {
        camTarget = targetWO;
        camOffsetBounds = targetWO.XZMaxExtent * camOffsetBoundsMult + camOffsetAdditional;
        camZoomDistance = targetWO.XZMaxExtent * 15.0f;
        UpdateCam();
    }
}
