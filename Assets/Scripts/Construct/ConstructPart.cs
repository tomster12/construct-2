using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConstructPart : MonoBehaviour
{
    [SerializeField] private ConstructMovement inherentMovement;

    private Construct currentConstruct;
    private IPartController currentController;
    private List<PartComponent> partComponents = new List<PartComponent>();

    private void Awake()
    {
        partComponents = GetComponents<PartComponent>().ToList();
    }

    public bool IsConstructed => currentConstruct != null;
    public bool IsControlled => currentController != null;

    public void OnJoinConstruct(Construct construct)
    {
        if (currentConstruct != null) throw new System.Exception("Cannot OnJoinConstruct(construct) when already joined to construct.");
        currentConstruct = construct;
        if (inherentMovement != null) currentConstruct.RegisterPartMovement(inherentMovement);
    }

    public void OnleaveConstruct()
    {
        if (currentConstruct == null) throw new System.Exception("Cannot OnleaveConstruct(construct) when not joined to construct.");
        if (inherentMovement != null) currentConstruct.DeregisterPartMovement(inherentMovement);
        currentConstruct = null;
    }

    public void SetController(IPartController controller)
    {
        if (currentController != null) throw new System.Exception("Cannot SetController(controller) when already have a controller.");
        currentController = controller;
    }
}
