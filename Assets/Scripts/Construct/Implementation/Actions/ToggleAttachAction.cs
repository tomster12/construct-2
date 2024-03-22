using UnityEngine;

public class ToggleAttachAction : ConstructAction
{
    public override string ActionName => "Mouse Skill 1"; // TODO: Some automatic way of binding skills
    public override ActionUseType UseType => ActionUseType.SINGLE;
    public override bool CanUse => base.CanUse && (CanAttach || CanDetach);
    public bool CanAttach => aimedAtachee != null && attacher.CanAttach(aimedAtachee);
    public bool CanDetach => attacher.CanDetach();
    public override bool IsActive => attacher.IsTransitioning;
    public override bool IsCooldown => false;

    public override void InputDown()
    {
        if (!CanUse) return;
        if (attacher.IsAttached && CanDetach) attacher.Detach();
        else if (CanAttach) attacher.Attach(aimedAtachee);
    }

    public override void InputUp()
    { }

    private Raycaster raycaster;
    private AttacherComponent attacher;
    private AttacheeComponent aimedAtachee;

    private void Awake()
    {
        attacher = GetComponent<AttacherComponent>();
        raycaster = new Raycaster(Camera.main); // TODO: Get from attacher.part.contruct...
    }

    private void Update()
    {
        raycaster.Update();
        aimedAtachee = raycaster.HitConstructPart?.GetPartComponent<AttacheeComponent>();
    }

    private void OnDrawGizmos()
    {
        if (raycaster == null) return;

        // TODO: Remove this debug GUI
        if (aimedAtachee != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(aimedAtachee.transform.position, 0.1f);
        }
        if (raycaster.Hit)
        {
            Gizmos.color = new Color(0.9f, 0.1f, 0.1f, 0.5f);
            Gizmos.DrawLine(transform.position, raycaster.HitPoint);
        }
    }
}
