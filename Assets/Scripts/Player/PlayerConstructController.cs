using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class PlayerConstructController : MonoBehaviour
{
    public Raycaster Raycaster => raycaster;
    public Construct Construct => construct;
    public static PlayerConstructController Instance;
    public UnityAction<PartConstruction[]> OnAvailableConstructionsChange = delegate { };

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
    private static readonly PlayerInput DECONSTRUCTION_BINDING = PlayerInput.KeyInput("g");

    [Header("References")]
    [SerializeField] private Camera camMain;
    [SerializeField] private Camera camSS;
    [SerializeField] private Transform camParent;
    [SerializeField] private Construct construct;
    [SerializeField] private ConstructPart corePart;
    [SerializeField] private RectTransform constructPartListUIParent;
    [SerializeField] private RectTransform constantReticle;
    [SerializeField] private RectTransform dynamicReticle;
    [SerializeField] private PlayerConstructPartPromptUI partPrompt;

    [Header("Prefabs")]
    [SerializeField] private GameObject constructPartIndicatorUIPrefab;
    [SerializeField] private GameObject constructPartUIPrefab;

    [Header("Config")]
    [SerializeField] private float camZoomAcc = 600.0f;
    [SerializeField] private float camZoomDrag = 0.95f;
    [SerializeField] private float camZoomVelMax = 0.1f;
    [SerializeField] private float reticleLerp = 5.0f;
    [SerializeField] private Vector3 camOffsetBoundsMult = new(1.0f, 0.5f, -1.0f);
    [SerializeField] private Vector3 camOffsetAdditional = new(1.0f, 0.0f, 0.0f);
    [SerializeField] private float camAimSpeed = 40.0f;
    [SerializeField] private float nearbyPartRadius = 5.0f;
    [SerializeField] private float constructPartListUIGap = 5.0f;
    [SerializeField] private float constructPartListUIHeight = 43.0f;
    [SerializeField] private float constructPartListUIPadding = 5.0f;

    private Vector3 movementInput;
    private Vector3 aimInput;
    private Raycaster raycaster;
    private PartConstruction[] availableConstructions = new PartConstruction[0];
    private WorldObject camTarget;
    private Vector3 camOffsetBounds;
    private float camZoomVel = 0.0f;
    private float camZoomDistance = 5.0f;
    private float camRotX = 0.0f;
    private float camRotY = 0.0f;
    private Dictionary<ConstructPart, ConstructPartIndicatorUI> partIndicatorUIs;
    private List<PlayerConstructPartUI> partListUIs = new();

    public void SetTarget(WorldObject targetWO)
    {
        camTarget = targetWO;
        camOffsetBounds = targetWO.MaxExtentXZ * camOffsetBoundsMult + camOffsetAdditional;
        camZoomDistance = targetWO.MaxExtentXZ * 15.0f;
        UpdateCamera();
    }

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    private void Start()
    {
        camMain.transform.parent = camParent;
        camSS.transform.parent = camParent;
        camMain.transform.localPosition = Vector3.zero;
        camSS.transform.localPosition = Vector3.zero;

        // Ensure it is empty on start
        foreach (Transform child in constructPartListUIParent) Destroy(child.gameObject);

        raycaster = new Raycaster(camMain);
        raycaster.OnTargetChange += OnRaycasterTargetChange;

        construct.OnPartEvent += OnConstructPartEvent;
        construct.OnSkillEvent += OnConstructSkillEvent;
        construct.OnMovementEvent += OnConstructMovementEvent;
        construct.OnActiveShapeEvent += OnConstructShapeEvent;
        construct.OnPrimaryMovementChange += OnConstructPrimaryMovementChange;
        construct.InitCore(corePart);

        partPrompt.Init(this);

        SetTarget(corePart.WO);
        LockMouse();
    }

    private void OnDestroy()
    {
        // TODO: Unsubscribe from events. PlayerConstructController will eventually not exist during main menu etc.
    }

    private void Update()
    {
        HandleInput();
        UpdateCamera();
        UpdatePartIndicators();
        UpdateReticle();
    }

    private async Task HandleInput()
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
            await construct.TryConstructPart(availableConstructions[0]);
        }
        if (DECONSTRUCTION_BINDING.GetDown())
        {
            construct.TryDeconstruct();
        }
    }

    private void UpdateCamera()
    {
        // Update cam zoom
        camZoomVel = Mathf.Clamp(camZoomVel, -camZoomVelMax, camZoomVelMax);
        camZoomDistance *= (1.0f + camZoomVel * Time.deltaTime);
        camZoomVel *= camZoomDrag;

        // Update cam rotation
        float diffRotX = -aimInput.y * camAimSpeed * Time.deltaTime;
        float diffRotY = aimInput.x * camAimSpeed * Time.deltaTime;
        camRotX = Mathf.Clamp(camRotX + diffRotX, -85.0f, 85.0f);
        camRotY += diffRotY;
        camParent.transform.localRotation = Quaternion.Euler(camRotX, camRotY, 0.0f);

        // Update cam positions
        Vector3 localPos = camOffsetBounds + camZoomDistance * Vector3.back;
        camParent.transform.position = camTarget.transform.position;
        camMain.transform.localPosition = localPos;
        camSS.transform.localPosition = localPos;

        // Update raycaster
        raycaster.Update();
    }

    private void UpdatePartIndicators()
    {
        // Find all nearby parts outside the construct
        partIndicatorUIs ??= new Dictionary<ConstructPart, ConstructPartIndicatorUI>();
        Vector3 centre = construct.GetCentre();
        foreach (ConstructPart part in ConstructPart.GlobalParts)
        {
            if (Vector3.Distance(centre, part.WO.transform.position) < nearbyPartRadius && !construct.Parts.Contains(part))
            {
                // Create an indicator if it doesn't exist
                if (!partIndicatorUIs.ContainsKey(part))
                {
                    ConstructPartIndicatorUI indicator = Instantiate(constructPartIndicatorUIPrefab, transform).GetComponent<ConstructPartIndicatorUI>();
                    indicator.Init(part);
                    partIndicatorUIs.Add(part, indicator);
                }

                // Highlight the indicator if it's targeted
                partIndicatorUIs[part].SetHighlighted(raycaster.HitConstructPart == part);
            }

            // Remove indicators for any parts that are no longer nearby
            else if (partIndicatorUIs.ContainsKey(part))
            {
                Destroy(partIndicatorUIs[part].gameObject);
                partIndicatorUIs.Remove(part);
            }
        }
    }

    private void UpdateAvailableConstructions()
    {
        // If targetting a part calculate available constructions with that part
        if (raycaster.HitConstructPart == null)
        {
            if (availableConstructions.Length != 0)
            {
                availableConstructions = new PartConstruction[0];
                OnAvailableConstructionsChange(availableConstructions);
            }
        }
        else
        {
            availableConstructions = construct.GetAvailableConstructions(raycaster.HitConstructPart);
            OnAvailableConstructionsChange(availableConstructions);
        }
    }

    private void UpdateReticle()
    {
        // Keep the constant reticle in the centre of the screen
        Vector3 target = new(Screen.width / 2, Screen.height / 2, 0.0f);
        constantReticle.anchoredPosition = target;

        // If highlighting a part then centre the dynamic reticle on the part
        if (raycaster.HitConstructPart != null) target = camMain.WorldToScreenPoint(raycaster.HitConstructPart.WO.transform.position);
        dynamicReticle.anchoredPosition = Vector2.Lerp(dynamicReticle.anchoredPosition, target, reticleLerp * Time.deltaTime);
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
        // Delete all the old part UIs
        foreach (PlayerConstructPartUI partUI in partListUIs) Destroy(partUI.gameObject);
        partListUIs.Clear();

        // Create a new part UI for each part in the construct
        for (int i = 0; i < Construct.Parts.Count; i++)
        {
            ConstructPart part = Construct.Parts[i];

            GameObject partUIObject = Instantiate(constructPartUIPrefab, constructPartListUIParent);
            RectTransform rectTfm = partUIObject.GetComponent<RectTransform>();
            PlayerConstructPartUI partUI = partUIObject.GetComponent<PlayerConstructPartUI>();

            // Manually calculate list offsets
            rectTfm.anchoredPosition = new Vector2(
                constructPartListUIPadding,
                constructPartListUIPadding + i * (constructPartListUIGap + constructPartListUIHeight));

            partUI.Init(part, this);
            partListUIs.Add(partUI);
        }
    }

    private void OnRaycasterTargetChange()
    {
        partPrompt.SetTarget(raycaster.HitConstructPart);
        UpdateAvailableConstructions();
    }

    private void OnConstructPartEvent(Construct.EventType type, ConstructPart part)
    {
        // When a part joins or leaves we want to redraw the part list UI
        Assert.IsTrue(type == Construct.EventType.Add || type == Construct.EventType.Remove);
        RedrawConstructPartListUI();

        // We then should also recalculate what available constructions we have
        UpdateAvailableConstructions();
    }

    private void OnConstructSkillEvent(Construct.EventType type, ConstructSkill skill)
    {
        // For now if a skill is added / removed we have no UI that depends on this so do nothing
    }

    private void OnConstructMovementEvent(Construct.EventType type, ConstructMovement movement)
    {
        // It is possible that a movement change can make constructions available / unavailable
        UpdateAvailableConstructions();
    }

    private void OnConstructShapeEvent(Construct.EventType type, ConstructShape shape, ConstructPart part)
    {
        // It is likely that a shape change can make constructions available / unavailable
        UpdateAvailableConstructions();
    }

    private void OnConstructPrimaryMovementChange(ConstructMovement movement)
    {
        // It is possible that a movement change can make constructions available / unavailable
        UpdateAvailableConstructions();
    }
}
