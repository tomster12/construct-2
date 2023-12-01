using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ToggleAttachAction))]
[RequireComponent(typeof(ConstructPart))]
public class AttacherComponent : PartComponent
{
    public bool IsAttached { get; private set; }
    public bool IsTransitioning { get; private set; }

    private ToggleAttachAction toggleAttachAction;
    private ConstructPart attacherPart;
    private IAttacherMovement attacherMovement;
    private AttachmentShape attachmentShape;

    private void Awake()
    {
        toggleAttachAction = GetComponent<ToggleAttachAction>();
        attacherPart = GetComponent<ConstructPart>();
        attacherMovement = (IAttacherMovement)attacherPart.Movement;
        attacherPart.OnJoinConstructEvent += OnJoinConstruct;
        attacherPart.OnLeaveConstructEvent += OnLeaveConstruct;
        attacherPart.OnPartChangeEvent += OnPartChange;
    }

    public void Attach(AttacheeComponent attachee) => StartCoroutine(IEAttach(attachee));

    public void Detach() => StartCoroutine(IEDetach());

    private IEnumerator IEAttach(AttacheeComponent attachee)
    {
        if (!CanAttach(attachee)) yield break;

        IsTransitioning = true;

        attachmentShape = gameObject.AddComponent<AttachmentShape>();
        attachmentShape.SetParts(attacherPart, attachee.AttacheePart);
        yield return attacherMovement.Attach(attachee);
        attacherMovement.SetControlling(false);
        attachmentShape.SetControlling(true);
        attacherPart.Construct.PickBestMovement();

        IsTransitioning = false;
        IsAttached = true;
    }

    private IEnumerator IEDetach()
    {
        if (!CanDetach()) yield break;

        IsTransitioning = true;

        attachmentShape.SetControlling(false);
        attacherMovement.SetControlling(true);
        yield return attacherMovement.Detach();
        Destroy(attachmentShape);
        attacherPart.Construct.PickBestMovement();

        IsTransitioning = false;
        IsAttached = false;
    }

    public bool CanAttach(AttacheeComponent attachee)
    {
        if (attachee == null) return false;
        if (IsTransitioning) return false;
        if (attacherMovement == null) return false;
        return attacherMovement.CanAttach(attachee);
    }

    public bool CanDetach()
    {
        if (IsTransitioning) return false;
        if (attacherMovement == null) return false;
        return attacherMovement.CanDetach() && attachmentShape != null && attachmentShape.CanSetControlling(false);
    }

    private void OnJoinConstruct(Construct construct)
    {
        construct.RegisterAction(toggleAttachAction);
    }

    private void OnLeaveConstruct()
    {
        attacherPart.Construct.DeregisterAction(toggleAttachAction);
    }

    private void OnPartChange()
    {
        attacherMovement = (IAttacherMovement)attacherPart.Movement;
    }

    private void OnDrawGizmos()
    {
        // TODO: Remove this debug GUI
        if (IsTransitioning)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(attachmentShape.AttachingPart.transform.position, attachmentShape.AttacheePart.transform.position);
        }
        else if (IsAttached)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(attachmentShape.AttachingPart.transform.position, attachmentShape.AttacheePart.transform.position);
        }
    }
}
