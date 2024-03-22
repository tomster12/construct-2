using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class AttacherComponent : PartComponent
{
    public bool IsAttached { get; private set; }
    public bool IsTransitioning { get; private set; }

    private ToggleAttachAction toggleAttachAction;
    private IAttacherMovement attacherMovement;
    private AttachmentShape attachmentShape;

    public override void Init(ConstructPart part)
    {
        base.Init(part);
        toggleAttachAction = gameObject.AddComponent<ToggleAttachAction>();
        attacherMovement = gameObject.GetComponent<IAttacherMovement>();
    }

    public void Attach(AttacheeComponent attachee) => StartCoroutine(IEAttach(attachee));

    public void Detach() => StartCoroutine(IEDetach());

    private IEnumerator IEAttach(AttacheeComponent attachee)
    {
        if (!CanAttach(attachee)) yield break;

        IsTransitioning = true;

        attachmentShape = gameObject.AddComponent<AttachmentShape>();
        attachmentShape.SetParts(Part, attachee.Part);

        yield return attacherMovement.Attach(attachee);
        attacherMovement.UnsetControlling();
        attachmentShape.SetControlling();
        Part.Construct.UpdateControllingMovement();

        IsTransitioning = false;
        IsAttached = true;
    }

    private IEnumerator IEDetach()
    {
        if (!CanDetach()) yield break;

        IsTransitioning = true;

        attachmentShape.UnsetControlling();
        attacherMovement.SetControlling();
        yield return attacherMovement.Detach();
        Part.Construct.UpdateControllingMovement();

        Destroy(attachmentShape);

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
        return attacherMovement.CanDetach() && attachmentShape != null && attachmentShape.CanUnsetControlling();
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
