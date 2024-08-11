using System.Linq;
using UnityEngine;

public abstract class ShapeUI : MonoBehaviour
{
    public virtual void Init()
    {
        PlayerConstructController.Instance.OnTargetChange += OnConstructTargetChange;
    }

    public abstract void Redraw();

    public virtual void OnConstructTargetChange()
    {
        // Find all player controller constructions involving this shape
        var relevantConstructions = PlayerConstructController.Instance.PossibleConstructions.Where(x => x.Item1 == shape);

        // Clear if there arent any
        if (relevantConstructions.Count() == 0) this.previewConstruction = null;

        // Otherwise grab the info from the first one
        else this.previewConstruction = (relevantConstructions.First().Item2, relevantConstructions.First().Item3);

        Redraw();
    }

    protected static Color DISABLED_COLOUR = new(0.5f, 0.5f, 0.5f);
    protected static Color ENABLED_COLOUR = new(1.0f, 1.0f, 1.0f);
    protected static Color HIGHLIGHT_COLOUR = new(0.9f, 0.55f, 0.2f);

    protected (ConstructPart part, int slot)? previewConstruction;
    protected abstract ConstructShape shape { get; }

    protected virtual void OnDestroy()
    {
        PlayerConstructController.Instance.OnTargetChange -= OnConstructTargetChange;
    }
}
