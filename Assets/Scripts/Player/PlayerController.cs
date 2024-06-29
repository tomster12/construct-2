using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public UnityAction OnTargetChange = delegate { };
    public Raycaster Raycaster => raycaster;
    public List<(ConstructShape, ConstructPart, int)> PossibleConstructions { get; private set; } = new();

    public void SetTarget(WorldObject targetWO)
    {
        camTarget = targetWO;
        camOffsetBounds = targetWO.XZMaxExtent * camOffsetBoundsMult + camOffsetAdditional;
        camZoomDistance = targetWO.XZMaxExtent * 15.0f;
        UpdateCamDynamics();
    }

    private static readonly Dictionary<PlayerInput, int> SKILL_BINDINGS = new Dictionary<PlayerInput, int>()
    {
        { PlayerInput.MouseInput(0), 0 },
        { PlayerInput.MouseInput(1), 1 },
        { PlayerInput.KeyInput("1"), 2 },
        { PlayerInput.KeyInput("2"), 3 },
        { PlayerInput.KeyInput("3"), 4 },
        { PlayerInput.KeyInput("4"), 5 },
    };

    private static readonly PlayerInput CONSTRUCTION_BINDING = PlayerInput.KeyInput("f");

    [Header("References")]
    [SerializeField] private Transform camParent;
    [SerializeField] private Camera cam;
    [SerializeField] private Construct construct;
    [SerializeField] private ConstructPart corePart;

    [Header("Prefabs")]
    [SerializeField] private GameObject partInspectorPrefab;

    [Header("Config")]
    [SerializeField] private float constructMoveSpeed = 10.0f;
    [SerializeField] private float camZoomAcc = 600.0f;
    [SerializeField] private float camZoomDrag = 0.9f;
    [SerializeField] private float camZoomVelMax = 6.0f;
    [SerializeField] private Vector3 camOffsetBoundsMult = new Vector3(1.0f, 0.5f, -1.0f);
    [SerializeField] private Vector3 camOffsetAdditional = new Vector3(1.0f, 0.0f, 0.0f);
    [SerializeField] private float camAimSpeed = 40.0f;
    [SerializeField] private float nearbyPartRadius = 5.0f;

    private Raycaster raycaster;
    private Vector3 movementInput;
    private Vector3 aimInput;
    private WorldObject camTarget;
    private Vector3 camOffsetBounds;
    private Vector3 camOffsetZoom;
    private float camZoomVel = 0.0f;
    private float camZoomDistance = 5.0f;
    private Dictionary<ConstructPart, PartInspectorUI> nearbyParts;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        cam.transform.parent = camParent;
        cam.transform.localPosition = Vector3.zero;
        raycaster = new Raycaster(cam);
        raycaster.OnTargetChange += OnRaycasterTargetChange;
        construct.InitCore(corePart);
        SetTarget(corePart.BaseWO);
        LockMouse();
    }

    private void Update()
    {
        HandleInput();
        UpdateCamDynamics();
        UpdatePartInspection();
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

        // Update skills and construction input
        foreach (KeyValuePair<PlayerInput, int> actionInput in SKILL_BINDINGS)
        {
            if (actionInput.Key.GetDown()) construct.SkillInputDown(actionInput.Value);
            else if (actionInput.Key.GetUp()) construct.SkillInputUp(actionInput.Value);
        }
        if (CONSTRUCTION_BINDING.GetDown() && PossibleConstructions.Count > 0)
        {
            construct.PerformConstruction(PossibleConstructions[0]);
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

    private void UpdatePartInspection()
    {
        nearbyParts ??= new Dictionary<ConstructPart, PartInspectorUI>();

        // Find all parts within radius and create inspector
        Vector3 centre = construct.GetCentre();
        foreach (ConstructPart part in ConstructPart.Parts)
        {
            if (Vector3.Distance(centre, part.BaseWO.transform.position) < nearbyPartRadius && !construct.Parts.Contains(part))
            {
                if (!nearbyParts.ContainsKey(part))
                {
                    PartInspectorUI inspector = Instantiate(partInspectorPrefab, transform).GetComponent<PartInspectorUI>();
                    inspector.Init(part);
                    nearbyParts.Add(part, inspector);
                }
            }
            else
            {
                if (nearbyParts.ContainsKey(part))
                {
                    Destroy(nearbyParts[part].gameObject);
                    nearbyParts.Remove(part);
                }
            }
        }
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

    private void OnRaycasterTargetChange()
    {
        UpdateConstructions();
        OnTargetChange();
    }

    private void UpdateConstructions()
    {
        // If not targetting clear and return
        if (raycaster.HitConstructPart == null)
        {
            PossibleConstructions.Clear();
            return;
        }

        // Find all construct shapes that targetted parts fits with
        foreach (ConstructShape shape in construct.Shapes)
        {
            (bool canConstruct, int slot) = shape.CanConstructWith(raycaster.HitConstructPart);
            if (canConstruct) PossibleConstructions.Add((shape, raycaster.HitConstructPart, slot));
        }

        // Find all part shapes which a construct part fits with
        foreach (ConstructPart part in construct.Parts)
        {
            foreach (ConstructShape shape in raycaster.HitConstructPart.Shapes)
            {
                (bool canConstruct, int slot) = shape.CanConstructWith(part);
                if (canConstruct) PossibleConstructions.Add((shape, part, slot));
            }
        }
    }
}
