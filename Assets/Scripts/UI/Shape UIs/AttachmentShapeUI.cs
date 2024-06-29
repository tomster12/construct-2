using UnityEngine;
using UnityEngine.Assertions;

public class AttachmentShapeUI : ShapeUI
{
    public void Init(AttachmentShape attachmentShape)
    {
        base.Init();
        this.attachmentShape = attachmentShape;
        attachmentShape.OnChange += Redraw;
        Redraw();
    }

    public override void Redraw()
    {
        // Set colours of attacher and attachee images
        if (previewConstruction != null) Assert.IsTrue(previewConstruction.Value.slot == 1);

        attacherImage.color = attachmentShape.AttacherPart != null ? ENABLED_COLOUR
            : previewConstruction != null ? HIGHLIGHT_COLOUR
            : DISABLED_COLOUR;

        attacheeImage.color = attachmentShape.AttacheePart != null ? ENABLED_COLOUR
            : DISABLED_COLOUR;
    }

    protected override ConstructShape shape => attachmentShape;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        attachmentShape.OnChange -= Redraw;
    }

    [Header("References")]
    [SerializeField] private UnityEngine.UI.Image attacherImage;
    [SerializeField] private UnityEngine.UI.Image attacheeImage;

    private AttachmentShape attachmentShape;
}
