using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class ConstructShapeUI : MonoBehaviour
{
    public static ConstructShapeUI Create(ConstructShape shape, Transform parent)
    {
        string prefabName;
        if (shape is AttachmentShape)
        {
            prefabName = "Construct Shape UI Attachment";
        }
        else
        {
            Debug.LogError("Unknown shape type: " + shape.GetType());
            return null;
        }

        GameObject shapeObject = Instantiate(AssetManager.GetPrefab(prefabName), parent);
        ConstructShapeUI shapeUI = shapeObject.GetComponent<ConstructShapeUI>();
        shapeUI.Init(shape);
        return shapeUI;
    }

    public void Init(ConstructShape shape)
    {
        this.shape = shape;
        this.shape.OnPartsChange += Redraw;
        PlayerConstructController.Instance.OnInspectedConstructionsChange += OnInspectedConstructionsChange;
        Redraw();
    }

    public void Redraw()
    {
        // Set colours of attacher and attachee images
        if (relevantPlayerConstruction != null) Assert.IsTrue(relevantPlayerConstruction.Value.slot == 1);

        for (int i = 0; i < slotImages.Length; i++)
        {
            slotImages[i].color = shape.GetSlot(i) != null ? ENABLED_COLOUR
                : (relevantPlayerConstruction != null && relevantPlayerConstruction.Value.slot == i) ? HIGHLIGHT_COLOUR
                : DISABLED_COLOUR;
        }
    }

    public void OnInspectedConstructionsChange(Construction[] constructions)
    {
        // Check whether player is considering this shape
        var relevantConstructions = constructions.Where(x => x.shape == shape);

        // Update variables with relevant construction
        if (relevantConstructions.Count() == 0)
        {
            relevantPlayerConstruction = null;
        }
        else
        {
            Assert.IsTrue(relevantConstructions.Count() == 1);
            relevantPlayerConstruction = relevantConstructions.First();
        }

        Redraw();
    }

    private static Color DISABLED_COLOUR = new(0.5f, 0.5f, 0.5f);
    private static Color ENABLED_COLOUR = new(1.0f, 1.0f, 1.0f);
    private static Color HIGHLIGHT_COLOUR = new(0.9f, 0.55f, 0.2f);

    [SerializeField] private Image[] slotImages;

    private Construction? relevantPlayerConstruction;
    private ConstructShape shape;

    private void OnDestroy()
    {
        PlayerConstructController.Instance.OnInspectedConstructionsChange -= OnInspectedConstructionsChange;
        shape.OnPartsChange -= Redraw;
        shape = null;
    }
}
