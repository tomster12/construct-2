using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class HoverMovement : ConstructMovement, IAttacherMovement
{
    public override void Move(Vector3 dir)
    {
        Assert.IsTrue(IsActive);

        // Move in direction with force
        isMoving = dir.magnitude > 0.1f;
        if (isMoving) partPH.AddWeightedForce(moveForce * Time.fixedDeltaTime * dir);
    }

    public override void Aim(Vector3 pos)
    {
        Assert.IsTrue(IsActive);

        // Aim towards position with torque
        Vector3 targetDir = pos - part.WO.transform.position;
        Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
        Quaternion rotDiff = targetRot * Quaternion.Inverse(part.WO.transform.rotation);
        rotDiff.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f)
        {
            angle = 360f - angle;
            axis = -axis;
        }
        partPH.AddWeightedTorque(aimForce * angle * Time.fixedDeltaTime * axis.normalized);
    }

    public override bool CanActivate() => !IsActive && part.CanControl && !isTransitioning;

    public override void Activate()
    {
        Assert.IsTrue(CanActivate());
        partPH = part.TakeControl(this);
        partPH.SetPhysicsMode(false, false);
        partPH.SetPhysicsProperties(hoverDrag, hoverAngularDrag);
        IsActive = true;
        OnStateChange.Invoke(IsActive);
    }

    public override void Deactivate()
    {
        Assert.IsTrue(IsActive);
        partPH.Release();
        IsActive = false;
        OnStateChange.Invoke(IsActive);
    }

    public IEnumerator EnumStartAttach(ConstructPart attacheePart, Action<bool> callback)
    {
        isTransitioning = true;

        // Place part on top of attachee part
        part.WO.transform.rotation = attacheePart.WO.transform.rotation;
        part.WO.transform.position = attacheePart.GetCentre()
            + (attacheePart.WO.Extents.y + part.WO.Extents.y - 0.2f) * attacheePart.WO.transform.up;
        part.WO.transform.SetParent(attacheePart.WO.transform);

        isTransitioning = false;
        callback(true);
        yield break;
    }

    public override Vector3 GetCentre() => part.GetCentre();

    [Header("References")]
    [SerializeField] private ConstructPart part;

    [Header("Config")]
    [SerializeField] private float hoverHeight = 1.0f;
    [SerializeField] private float oscillateMagnitude = 0.1f;
    [SerializeField] private float oscillateSpeed = 1.0f;
    [SerializeField] private float moveForce = 1.0f;
    [SerializeField] private float hoverForce = 2.0f;
    [SerializeField] private float hoverDrag = 0.5f;
    [SerializeField] private float hoverAngularDrag = 0.5f;
    [SerializeField] private float fallForce = 1.0f;
    [SerializeField] private float tiltForce = 0.1f;
    [SerializeField] private float aimForce = 0.1f;
    [SerializeField] private float aimThreshold = 0.05f;
    [SerializeField] private float uprightForce = 0.1f;
    [SerializeField] private float uprightThreshold = 0.05f;

    private ConstructPart.PhysicalHandle partPH;
    private bool isGrounded;
    private bool isMoving;
    private bool isTransitioning;
    private Vector3 groundPosition;
    private float groundTime;
    private float maxHoverHeight => hoverHeight * 2.0f;

    private void Awake()
    {
        Assert.IsTrue(part != null);
    }

    private void FixedUpdate()
    {
        if (!IsActive) return;

        float targetY;

        // Oscillate above closest reasonable surface
        LayerMask layer = LayerMask.GetMask("Terrain");
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxHoverHeight * 1.5f, layer))
        {
            if (!isGrounded) groundTime = Time.time;
            isGrounded = true;
            groundPosition = hit.point;
            targetY = groundPosition.y + GetTargetHoverHeight(Time.time - groundTime);
        }

        // Float downwards otherwise
        else
        {
            isGrounded = false;
            groundPosition = Vector3.zero;
            targetY = transform.position.y - 1f;
        }

        // Apply hover force twards target
        Vector3 targetPosition = new Vector3(transform.position.x, targetY, transform.position.z);
        Vector3 force = (targetPosition - transform.position) * (isGrounded ? hoverForce : fallForce);
        part.WO.RB.AddForce(force);

        // Lean towards RB with a torque
        if (isMoving)
        {
            Vector3 axis = -Vector3.Cross(part.WO.RB.velocity, Vector3.up).normalized;
            partPH.AddWeightedTorque(axis * tiltForce);
        }
    }

    private float GetTargetHoverHeight(float t) => hoverHeight + Mathf.Sin(t * oscillateSpeed) * oscillateMagnitude;
}
