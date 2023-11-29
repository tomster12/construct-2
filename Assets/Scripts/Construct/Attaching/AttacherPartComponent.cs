using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ToggleAttachAction))]
[RequireComponent(typeof(ConstructPart))]
[RequireComponent(typeof(IAttacherMovement))]
public class AttacherPartComponent : PartComponent
{
    public bool IsAttached { get; private set; }
    public bool IsTransitioning { get; private set; }
    public bool CanToggle => !IsTransitioning;

    private ToggleAttachAction toggleAttachAction;
    private ConstructPart attacherPart;
    private IAttacherMovement attacherMovement;

    private void Awake()
    {
        toggleAttachAction = GetComponent<ToggleAttachAction>();
        attacherPart = GetComponent<ConstructPart>();
        attacherMovement = GetComponent<IAttacherMovement>();
        attacherPart.OnJoinConstructEvent += OnJoinConstruct;
        attacherPart.OnLeaveConstructEvent += OnLeaveConstruct;
    }

    public void ToggleAttach(AttacheePartComponent attachee) => StartCoroutine(_ToggleAttach(attachee));

    private IEnumerator _ToggleAttach(AttacheePartComponent attachee)
    {
        if (!CanToggle) yield break;
        if (!CanToggleAttach(attachee)) yield break;

        IsTransitioning = true;
        if (!IsAttached) yield return attacherMovement.Attach(attachee);
        else yield return attacherMovement.Detach();
        IsTransitioning = false;
        IsAttached = !IsAttached;
    }

    public bool CanToggleAttach(AttacheePartComponent attachee)
    {
        if (IsAttached) return attacherMovement.CanAttach(attachee);
        else return attacherMovement.CanDetach();
    }

    private void OnJoinConstruct(Construct construct)
    {
        construct.RegisterAction(toggleAttachAction);
    }

    private void OnLeaveConstruct()
    {
        attacherPart.Construct.DeregisterAction(toggleAttachAction);
    }
}
