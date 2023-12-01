using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCamera))]
public class PlayerController : MonoBehaviour
{
    private static readonly Dictionary<PlayerInput, string> ACTION_INPUTS = new Dictionary<PlayerInput, string>()
    {
        { PlayerInput.MouseInput(0), "Attach" }
    };

    [SerializeField] Construct construct;
    [SerializeField] ConstructPart corePart;

    private PlayerCamera playerCamera;
    private Raycaster raycaster = new Raycaster();
    private Vector3 inputDir;

    private void Awake()
    {
        playerCamera = GetComponent<PlayerCamera>();
    }

    private void Start()
    {
        construct.SetCore(corePart);
        playerCamera.SetTarget(corePart.WO);
    }

    private void Update()
    {
        HandleInput();
        HandleActions();
    }

    private void FixedUpdate()
    {
        FixedHandleMovement();
    }

    private void HandleInput()
    {
        inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
    }

    private void HandleActions()
    {
        foreach (KeyValuePair<PlayerInput, string> actionInput in ACTION_INPUTS)
        {
            if (actionInput.Key.GetDown())
            {
                construct.UseDownAction(actionInput.Value);
            }
            else if (actionInput.Key.GetUp())
            {
                construct.UseUpAction(actionInput.Value);
            }
        }
        construct.VisualiseAllActions();
    }

    private void FixedHandleMovement()
    {
        raycaster.Update();
        construct.Move(inputDir);
        construct.Aim(raycaster.HitPoint);
    }
}
