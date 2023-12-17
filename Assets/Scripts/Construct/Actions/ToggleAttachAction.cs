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

    private Raycaster raycaster = new Raycaster();
    private AttacherComponent attacher;
    private AttacheeComponent aimedAtachee;

    private void Awake()
    {
        attacher = GetComponent<AttacherComponent>();
    }

    private void Update()
    {
        raycaster.Update();
        if (raycaster.HitConstructPart) aimedAtachee = raycaster.HitConstructPart.GetPartComponent<AttacheeComponent>();
        else aimedAtachee = null;
    }

    public override void InputDown()
    {
        if (!CanUse) return;
        if (attacher.IsAttached && CanDetach) attacher.Detach();
        else if (CanAttach) attacher.Attach(aimedAtachee);
    }

    public override void InputUp() { }

    private void OnDrawGizmos()
    {
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
