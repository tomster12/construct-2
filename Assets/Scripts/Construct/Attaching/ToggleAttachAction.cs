using UnityEngine;

[RequireComponent(typeof(AttacherPartComponent))]
public class ToggleAttachAction : ConstructAction
{
    public override string ActionName => "Attach";
    public override ActionUseType UseType => ActionUseType.SINGLE;
    public override bool CanUse => base.CanUse && attacher.CanToggleAttach(aimedAtachee);
    public override bool IsActive => attacher.IsTransitioning;
    public override bool IsCooldown => false;

    private AttacherPartComponent attacher;
    private AttacheePartComponent aimedAtachee;

    private void Awake()
    {
        attacher = GetComponent<AttacherPartComponent>();
    }

    private void Update()
    {
        // if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        // {
        //     aimedAtachee = hit.collider.GetComponent<AttacheePartComponent>();
        // }
    }

    public override void UseDown()
    {
        if (!CanUse) return;
        attacher.ToggleAttach(aimedAtachee);
    }

    public override void UseUp() { }

    public override void Visualise() { }
}
