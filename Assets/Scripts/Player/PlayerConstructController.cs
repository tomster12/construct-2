using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class PlayerConstructController : MonoBehaviour
{
    public static PlayerConstructController Instance;

    public UnityAction<Construction[]> OnAvailableConstructionsChange { get; set; } = delegate { };
    public Raycaster Raycaster => raycaster;
    public Construct Construct => construct;

    public void SetTarget(WorldObject targetWO)
    {
        camTarget = targetWO;
        camOffsetBounds = targetWO.MaxExtentXZ * camOffsetBoundsMult + camOffsetAdditional;
        camZoomDistance = targetWO.MaxExtentXZ * 15.0f;
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
    [SerializeField] private Camera cam;
    [SerializeField] private Transform camParent;
    [SerializeField] private Construct construct;
    [SerializeField] private ConstructPart corePart;
    [SerializeField] private RectTransform constructPartUIListParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject constructPartIndicatorUIPrefab;
    [SerializeField] private GameObject constructPartUIPrefab;

    [Header("Config")]
    [SerializeField] private float camZoomAcc = 600.0f;
    [SerializeField] private float camZoomDrag = 0.95f;
    [SerializeField] private float camZoomVelMax = 0.1f;
    [SerializeField] private Vector3 camOffsetBoundsMult = new Vector3(1.0f, 0.5f, -1.0f);
    [SerializeField] private Vector3 camOffsetAdditional = new Vector3(1.0f, 0.0f, 0.0f);
    [SerializeField] private float camAimSpeed = 40.0f;
    [SerializeField] private float nearbyPartRadius = 5.0f;
    [SerializeField] private float constructPartListUIGap = 5.0f;
    [SerializeField] private float constructPartListUIHeight = 43.0f;
    [SerializeField] private float constructPartListUIPadding = 5.0f;

    private Raycaster raycaster;
    private Vector3 movementInput;
    private Vector3 aimInput;
    private WorldObject camTarget;
    private Vector3 camOffsetBounds;
    private float camZoomVel = 0.0f;
    private float camZoomDistance = 5.0f;
    private Dictionary<ConstructPart, ConstructPartIndicatorUI> nearbyParts;
    private Construction[] availableConstructions = new Construction[0];

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    private void Start()
    {
        cam.transform.parent = camParent;
        cam.transform.localPosition = Vector3.zero;
        raycaster = new Raycaster(cam);
        raycaster.OnTargetChange += OnRaycasterTargetChange;
        construct.OnConstructChange += OnConstructChange;
        construct.InitCore(corePart);
        SetTarget(corePart.WO);
        LockMouse();
    }

    private void Update()
    {
        HandleInput();
        UpdateCamDynamics();
        UpdateNearbyPartIndicatorUIs();
    }

    private void OnDestroy()
    {
        construct.OnConstructChange -= OnConstructChange;
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
        if (CONSTRUCTION_BINDING.GetDown() && availableConstructions.Length > 0)
        {
            construct.PerformConstruction(availableConstructions[0]);
        }
    }

    private void UpdateCamDynamics()
    {
        // Update cam zoom
        camZoomVel = Mathf.Clamp(camZoomVel, -camZoomVelMax, camZoomVelMax);
        camZoomDistance *= (1.0f + camZoomVel * Time.deltaTime);
        camZoomVel *= camZoomDrag;

        // Update cam rotation
        camParent.transform.RotateAround(camParent.transform.position, Vector3.up, aimInput.x * Time.deltaTime * camAimSpeed);
        camParent.transform.RotateAround(camParent.transform.position, camParent.transform.right, -aimInput.y * Time.deltaTime * camAimSpeed);

        // Update cam position
        camParent.transform.position = camTarget.transform.position;
        cam.transform.localPosition = camOffsetBounds + camZoomDistance * Vector3.back;

        // Update raycaster
        raycaster.Update();
    }

    private void UpdateNearbyPartIndicatorUIs()
    {
        nearbyParts ??= new Dictionary<ConstructPart, ConstructPartIndicatorUI>();

        Vector3 centre = construct.GetCentre();
        foreach (ConstructPart part in ConstructPart.GlobalParts)
        {
            // If part is close enough create an indicator UI
            if (Vector3.Distance(centre, part.WO.transform.position) < nearbyPartRadius && !construct.Parts.Contains(part))
            {
                if (!nearbyParts.ContainsKey(part))
                {
                    ConstructPartIndicatorUI indicator = Instantiate(constructPartIndicatorUIPrefab, transform).GetComponent<ConstructPartIndicatorUI>();
                    indicator.Init(part);
                    nearbyParts.Add(part, indicator);
                }
            }

            // Otherwise remove the UI if it exists
            else if (nearbyParts.ContainsKey(part))
            {
                Destroy(nearbyParts[part].gameObject);
                nearbyParts.Remove(part);
            }
        }
    }

    private void UpdateAvailableConstructions()
    {
        // If not targetting clear and return
        if (raycaster.HitConstructPart == null) availableConstructions = new Construction[0];
        else availableConstructions = construct.GetAvailableConstructions(raycaster.HitConstructPart);
        OnAvailableConstructionsChange(availableConstructions);
    }

    private void FixedUpdate()
    {
        FixedUpdateConstruct();
    }

    private void FixedUpdateConstruct()
    {
        construct.Move(movementInput);
        construct.Aim(raycaster.HitPoint);
    }

    private void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void RedrawConstructPartListUI()
    {
        foreach (Transform child in constructPartUIListParent)

        {
            Destroy(child.gameObject);
        }

        // Create a new part UI for each part in the construct
        for (int i = 0; i < Construct.Parts.Count; i++)
        {
            ConstructPart part = Construct.Parts[i];

            GameObject partUIObject = Instantiate(constructPartUIPrefab, constructPartUIListParent);
            RectTransform rectTfm = partUIObject.GetComponent<RectTransform>();
            ConstructPartUI partUI = partUIObject.GetComponent<ConstructPartUI>();
            rectTfm.anchoredPosition = new Vector2(
                constructPartListUIPadding,
                constructPartListUIPadding + i * (constructPartListUIGap + constructPartListUIHeight));

            partUI.Init(part);
        }
    }

    private void OnRaycasterTargetChange()
    {
        UpdateAvailableConstructions();
    }

    private void OnConstructChange()
    {
        RedrawConstructPartListUI();
    }
}
