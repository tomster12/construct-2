using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public void SetTarget(WorldObject targetWO)
    {
        camTarget = targetWO;
        camOffsetBounds = targetWO.XZMaxExtent * camOffsetBoundsMult + camOffsetAdditional;
        camZoomDistance = targetWO.XZMaxExtent * 15.0f;
        UpdateCamDynamics();
    }

    private static readonly Dictionary<PlayerInput, string> ACTION_INPUT_BINDINGS = new Dictionary<PlayerInput, string>()
    {
        { PlayerInput.MouseInput(0), "Mouse Skill 1" },
        { PlayerInput.MouseInput(1), "Mouse Skill 2" },
        { PlayerInput.KeyInput("1"), "Skill 1" },
        { PlayerInput.KeyInput("2"), "Skill 2" },
        { PlayerInput.KeyInput("3"), "Skill 3" },
        { PlayerInput.KeyInput("4"), "Skill 4" },
    };

    [SerializeField] private Transform camParent;
    [SerializeField] private Camera cam;
    [SerializeField] private Construct construct;
    [SerializeField] private ConstructPart corePart;

    [Header("Config")]
    [SerializeField] private float constructMoveSpeed = 10.0f;
    [SerializeField] private float camZoomAcc = 600.0f;
    [SerializeField] private float camZoomDrag = 0.9f;
    [SerializeField] private float camZoomVelMax = 6.0f;
    [SerializeField] private Vector3 camOffsetBoundsMult = new Vector3(1.0f, 0.5f, -1.0f);
    [SerializeField] private Vector3 camOffsetAdditional = new Vector3(1.0f, 0.0f, 0.0f);
    [SerializeField] private float camAimSpeed = 40.0f;

    private Raycaster raycaster;
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
        cam.transform.localPosition = Vector3.zero;
        raycaster = new Raycaster(cam);
        construct.InitCore(corePart);
        SetTarget(corePart.WO);
        LockMouse();
    }

    private void Update()
    {
        HandleInput();
        UpdateCamDynamics();
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

        // Update actions
        foreach (KeyValuePair<PlayerInput, string> actionInput in ACTION_INPUT_BINDINGS)
        {
            if (actionInput.Key.GetDown()) construct.ActionInputDown(actionInput.Value);
            else if (actionInput.Key.GetUp()) construct.ActionInputUp(actionInput.Value);
        }
    }

    private void UpdateCamDynamics()
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
        FixedUpdateConstruct();
    }

    private void FixedUpdateConstruct()
    {
        construct.Move(movementInput * Time.fixedDeltaTime * constructMoveSpeed);
        construct.Aim(raycaster.HitPoint);
    }

    private void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
