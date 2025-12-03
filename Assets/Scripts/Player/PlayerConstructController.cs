using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

public partial class PlayerConstructController : MonoBehaviour
{
    public enum StateType
    { None, Playing, Forging }

    public class BaseState
    {
        public virtual void Init(PlayerConstructController controller)
        {
            this.controller = controller;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }

        protected PlayerConstructController controller;
    }

    [Header("References")]
    [SerializeField] private Construct construct;
    [SerializeField] private ConstructPart corePart;
    [SerializeField] private Transform camParent;
    [SerializeField] private Camera camMain;
    [SerializeField] private Camera camSS;
    [SerializeField] private RectTransform constantReticle;
    [SerializeField] private RectTransform dynamicReticle;
    [SerializeField] private PlayerConstructPartPromptUI partPrompt;

    [Header("States")]
    [SerializeField] private PlayingState playingState;
    [SerializeField] private ForgingState forgingState;

    private Dictionary<StateType, BaseState> states = new();
    private StateType currentStateType = StateType.None;
    private BaseState currentState = null;
    private Raycaster raycaster = null;
    private UnityAction OnRaycasterTargetChange = delegate { };

    private void Start()
    {
        camMain.transform.parent = camParent;
        camSS.transform.parent = camParent;
        camMain.transform.localPosition = Vector3.zero;
        camSS.transform.localPosition = Vector3.zero;

        raycaster = new Raycaster(camMain);
        raycaster.OnTargetChange += OnRaycasterTargetChange;

        partPrompt.Init(playingState);

        states[StateType.Playing] = playingState;
        states[StateType.Forging] = forgingState;
        playingState.Init(this);
        forgingState.Init(this);

        Transition(StateType.Playing);
    }

    private void Update()
    {
        currentState.Update();
    }

    private void FixedUpdate()
    {
        currentState.FixedUpdate();
    }

    private void Transition(StateType stateType)
    {
        if (currentStateType == stateType) return;

        currentState?.Exit();
        currentStateType = stateType;
        currentState = states[stateType];
        currentState.Enter();
    }

    private void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

public partial class PlayerConstructController
{
    [Serializable]
    public class PlayingState : BaseState
    {
        public UnityAction<PartConstruction[]> OnAvailableConstructionsChange = delegate { };

        private static readonly Dictionary<PlayerInput, int> SKILL_BINDINGS = new()
        {   { PlayerInput.MouseInput(0), 0 },
            { PlayerInput.MouseInput(1), 1 },
            { PlayerInput.KeyInput("1"), 2 },
            { PlayerInput.KeyInput("2"), 3 },
            { PlayerInput.KeyInput("3"), 4 },
            { PlayerInput.KeyInput("4"), 5 }, };
        private static readonly PlayerInput CONSTRUCTION_BINDING = PlayerInput.KeyInput("f");
        private static readonly PlayerInput DECONSTRUCTION_BINDING = PlayerInput.KeyInput("g");

        [Header("References")]
        [SerializeField] private RectTransform constructPartListUIParent;

        [Header("Prefabs")]
        [SerializeField] private GameObject constructPartIndicatorUIPrefab;
        [SerializeField] private GameObject constructPartUIPrefab;

        [Header("Config")]
        [SerializeField] private float camAimSpeed = 40.0f;
        [SerializeField] private float camZoomAcc = 600.0f;
        [SerializeField] private float camZoomDrag = 0.95f;
        [SerializeField] private float camZoomVelMax = 0.1f;
        [SerializeField] private Vector3 camOffsetBoundsMult = new(1.0f, 0.5f, -1.0f);
        [SerializeField] private Vector3 camOffsetAdditional = new(1.0f, 0.0f, 0.0f);
        [SerializeField] private float reticleLerp = 5.0f;
        [SerializeField] private float nearbyPartRadius = 5.0f;
        [SerializeField] private float constructPartListUIGap = 5.0f;
        [SerializeField] private float constructPartListUIHeight = 43.0f;
        [SerializeField] private float constructPartListUIPadding = 5.0f;

        private PartConstruction[] availableConstructions = new PartConstruction[0];
        private Vector3 movementInput;
        private Vector3 aimInput;
        private Vector3 camOffsetBounds;
        private Transform camTarget;
        private float camZoomVelocity = 0.0f;
        private float camZoomDistance = 5.0f;
        private Vector2 camRotation = Vector2.zero;
        private Dictionary<ConstructPart, ConstructPartIndicatorUI> partIndicatorUIs;
        private List<PlayerConstructPartUI> partListUIs = new();

        public override void Enter()
        {
            controller.OnRaycasterTargetChange += OnRaycasterTargetChange;

            foreach (Transform child in constructPartListUIParent) Destroy(child.gameObject);

            controller.construct.OnPartEvent += OnConstructPartEvent;
            controller.construct.OnSkillEvent += OnConstructSkillEvent;
            controller.construct.OnMovementEvent += OnConstructMovementEvent;
            controller.construct.OnActiveShapeEvent += OnConstructShapeEvent;
            controller.construct.OnPrimaryMovementChange += OnConstructPrimaryMovementChange;

            if (!controller.construct.IsInitialized) controller.construct.InitCore(controller.corePart);

            SetCameraTarget(controller.corePart.WO);
            controller.LockMouse();
        }

        public override void Update()
        {
            HandleInput();
            UpdateCamera();
        }

        private void HandleInput()
        {
            // Update input direction
            movementInput = Vector3.zero;
            Vector3 flatForward = Vector3.ProjectOnPlane(controller.camParent.forward, Vector3.up).normalized;
            movementInput += controller.camParent.right * Input.GetAxisRaw("Horizontal");
            movementInput += flatForward * Input.GetAxisRaw("Vertical");

            // Update zoom velocity
            camZoomVelocity += -Input.mouseScrollDelta.y * camZoomAcc * Time.deltaTime;

            // Update aiming
            aimInput = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"), 0.0f);

            // Update skills input
            foreach (KeyValuePair<PlayerInput, int> actionInput in SKILL_BINDINGS)
            {
                if (actionInput.Key.GetDown()) controller.construct.SkillInputDown(actionInput.Value);
                else if (actionInput.Key.GetUp()) controller.construct.SkillInputUp(actionInput.Value);
            }

            // Handle construction / deconstruction (ignoring async tasks)
            if (CONSTRUCTION_BINDING.GetDown() && availableConstructions.Length > 0)
            {
                _ = controller.construct.TryConstructPart(availableConstructions[0]);
            }
            if (DECONSTRUCTION_BINDING.GetDown())
            {
                _ = controller.construct.TryDeconstruct();
            }
        }

        private void UpdateCamera()
        {
            // Calculate camera zoom, position, and rotation
            camZoomVelocity = Mathf.Clamp(camZoomVelocity, -camZoomVelMax, camZoomVelMax);
            camZoomDistance *= (1.0f + camZoomVelocity * Time.deltaTime);
            camZoomVelocity *= camZoomDrag;
            Vector3 localPos = camOffsetBounds + camZoomDistance * Vector3.back;
            float diffRotX = -aimInput.y * camAimSpeed * Time.deltaTime;
            float diffRotY = aimInput.x * camAimSpeed * Time.deltaTime;
            camRotation.x = Mathf.Clamp(camRotation.x + diffRotX, -85.0f, 85.0f);
            camRotation.y += diffRotY;

            // Update controller cameras
            controller.camParent.transform.position = camTarget.transform.position;
            controller.camMain.transform.localPosition = localPos;
            controller.camSS.transform.localPosition = localPos;
            controller.camParent.transform.localRotation = Quaternion.Euler(camRotation.x, camRotation.y, 0.0f);

            // Update controller raycaster
            controller.raycaster.Update();
        }

        public override void FixedUpdate()
        {
            FixedUpdateConstruct();
        }

        private void FixedUpdateConstruct()
        {
            controller.construct.Move(movementInput);
            controller.construct.Aim(controller.raycaster.HitPoint);
        }

        private void SetCameraTarget(WorldObject targetWO)
        {
            camOffsetBounds = targetWO.MaxExtentXZ * camOffsetBoundsMult + camOffsetAdditional;
            camZoomDistance = targetWO.MaxExtentXZ * 15.0f;
            camTarget = targetWO.transform;
            UpdateCamera();
        }

        private void UpdatePartIndicators()
        {
            // Find all nearby parts outside the construct
            partIndicatorUIs ??= new Dictionary<ConstructPart, ConstructPartIndicatorUI>();
            Vector3 centre = controller.construct.GetCentre();
            foreach (ConstructPart part in ConstructPart.GlobalParts)
            {
                if (Vector3.Distance(centre, part.WO.transform.position) < nearbyPartRadius && !controller.construct.Parts.Contains(part))
                {
                    // Create an indicator if it doesn't exist
                    if (!partIndicatorUIs.ContainsKey(part))
                    {
                        ConstructPartIndicatorUI indicator = Instantiate(constructPartIndicatorUIPrefab, controller.transform).GetComponent<ConstructPartIndicatorUI>();
                        indicator.Init(part);
                        partIndicatorUIs.Add(part, indicator);
                    }

                    // Highlight the indicator if it's targeted
                    partIndicatorUIs[part].SetHighlighted(controller.raycaster.HitConstructPart == part);
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
            if (controller.raycaster.HitConstructPart == null)
            {
                if (availableConstructions.Length != 0)
                {
                    availableConstructions = new PartConstruction[0];
                    OnAvailableConstructionsChange(availableConstructions);
                }
            }
            else
            {
                availableConstructions = controller.construct.GetAvailableConstructions(controller.raycaster.HitConstructPart);
                OnAvailableConstructionsChange(availableConstructions);
            }
        }

        private void UpdateReticle()
        {
            // Keep the constant reticle in the centre of the screen
            Vector3 target = new(Screen.width / 2, Screen.height / 2, 0.0f);
            controller.constantReticle.anchoredPosition = target;

            // If highlighting a part then centre the dynamic reticle on the part
            if (controller.raycaster.HitConstructPart != null) target = controller.camMain.WorldToScreenPoint(controller.raycaster.HitConstructPart.WO.transform.position);
            controller.dynamicReticle.anchoredPosition = Vector2.Lerp(controller.dynamicReticle.anchoredPosition, target, reticleLerp * Time.deltaTime);
        }

        private void RedrawConstructPartListUI()
        {
            // Delete all the old part UIs
            foreach (PlayerConstructPartUI partUI in partListUIs) Destroy(partUI.gameObject);
            partListUIs.Clear();

            // Create a new part UI for each part in the construct
            for (int i = 0; i < controller.construct.Parts.Count; i++)
            {
                ConstructPart part = controller.construct.Parts[i];

                GameObject partUIObject = Instantiate(constructPartUIPrefab, constructPartListUIParent);
                RectTransform rectTfm = partUIObject.GetComponent<RectTransform>();
                PlayerConstructPartUI partUI = partUIObject.GetComponent<PlayerConstructPartUI>();

                // Manually calculate list offsets
                rectTfm.anchoredPosition = new Vector2(
                    constructPartListUIPadding,
                    constructPartListUIPadding + i * (constructPartListUIGap + constructPartListUIHeight));

                partUI.Init(part, controller.playingState);
                partListUIs.Add(partUI);
            }
        }

        private void OnRaycasterTargetChange()
        {
            controller.partPrompt.SetTarget(controller.raycaster.HitConstructPart);
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
}

public partial class PlayerConstructController
{
    [Serializable]
    public class ForgingState : BaseState
    {
    }
}
