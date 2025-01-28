using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerConstructPartUI : MonoBehaviour
{
    public void Init(ConstructPart part, PlayerConstructController player)
    {
        this.part = part;
        this.player = player;

        // Ensure it is empty on start
        foreach (Transform child in shapesParent.transform) Destroy(child.gameObject);

        RedrawProperties();
        RedrawShapes();

        this.part.OnShapeEvent += OnPartShapeEvent;
        this.player.OnAvailableConstructionsChange += OnAvailableConstructionsChange;
    }

    [Header("References")]
    [SerializeField] private TMPro.TextMeshProUGUI levelText;
    [SerializeField] private TMPro.TextMeshProUGUI nameText;
    [SerializeField] private GameObject shapesParent;

    private ConstructPart part;
    private PlayerConstructController player;
    private List<PlayerConstructShapeUI> shapes = new();
    private Dictionary<ConstructShape, Construction> constructionsMyShapeOtherPart = new();
    private List<Construction> constructionsOtherShapeThisPart = new();

    private void OnDestroy()
    {
        part.OnShapeEvent -= OnPartShapeEvent;
        player.OnAvailableConstructionsChange -= OnAvailableConstructionsChange;
    }

    private void RedrawProperties()
    {
        levelText.text = part.Level.ToString();
        nameText.text = part.gameObject.name;
    }

    private void RedrawShapes()
    {
        // For now we are just re-instantiating all shape UIs
        // The better way to do this would be to figure out what shapes are added / removed / changed
        // This works the same for the suggested constructions

        // Destroy all old shape UIs
        foreach (PlayerConstructShapeUI shapeUI in shapes) Destroy(shapeUI.gameObject);
        shapes.Clear();

        // Create new shape UIs
        foreach (ConstructShape shape in part.Shapes)
        {
            PlayerConstructShapeUI shapeUI = PlayerConstructShapeUI.Create(shape, shapesParent.transform);
            shapes.Add(shapeUI);

            // Set suggested construction if there is one
            if (constructionsMyShapeOtherPart.TryGetValue(shape, out Construction construction))
            {
                shapeUI.SetSuggestedConstruction(construction);
            }
        }

        // Create a shape UI for each suggested construction this part can fit into
        foreach (Construction construction in constructionsOtherShapeThisPart)
        {
            PlayerConstructShapeUI shapeUI = PlayerConstructShapeUI.Create(construction.shape, shapesParent.transform);
            shapes.Add(shapeUI);
            shapeUI.SetSuggestedConstruction(construction);
            shapeUI.SetIsSuggestion(true);
        }
    }

    private void OnPartShapeEvent(ConstructPart part, ConstructPart.EventType type, ConstructShape shape)
    {
        // Completely redraw shapes whenever any added / removed / changed
        RedrawShapes();
    }

    private void OnAvailableConstructionsChange(Construction[] constructions)
    {
        constructionsMyShapeOtherPart.Clear();
        constructionsOtherShapeThisPart.Clear();

        // We need to update the colours on the relevant shape UIs if another part can fit in them
        foreach (Construction construction in constructions)
        {
            if (part.Shapes.Contains(construction.shape))
            {
                constructionsMyShapeOtherPart.Add(construction.shape, construction);
            }
        }

        // We need to create new shape UIs for each shape that this part can fit in
        foreach (Construction construction in constructions)
        {
            if (construction.part == part)
            {
                constructionsOtherShapeThisPart.Add(construction);
            }
        }

        RedrawShapes();
    }
}
