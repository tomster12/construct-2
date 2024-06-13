using UnityEngine;

public class HopMovement : MonoBehaviour, IConstructMovement
{
    [SerializeField] private ConstructPart part;

    public bool IsControlling { get; private set; }
    public bool CanTransition => IsGrounded;
    public bool IsGrounded { get; private set; }

    // TODO: Implement
    public void Aim(Vector3 pos) => Debug.LogWarning("HopMovement.Aim(pos) not implemented.");

    // TODO: Implement
    public void Move(Vector3 dir) => Debug.LogWarning("HopMovement.Move(dir) not implemented.");

    public void SetControlling()
    {
        if (!CanSetControlling()) throw new System.Exception("Cannot SetControlling(true) when already controlled.");
        IsControlling = true;
        part.SetController(this);
    }

    public void UnsetControlling()
    {
        if (!CanUnsetControlling()) throw new System.Exception("Cannot UnsetControlling(false) when not controlled.");
        IsControlling = false;
        part.UnsetController();
    }

    public bool CanSetControlling() => CanTransition;

    public bool CanUnsetControlling() => CanTransition;

    public bool CanEnterForging() => false;

    public bool CanExitForging() => true;
}
