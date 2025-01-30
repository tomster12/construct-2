using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerConstructPartPromptUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform constructionPrompt;
    [SerializeField] private GameObject shapesParent;
    [SerializeField] private HorizontalLayoutGroup shapesLayout;

    private ConstructPart targetPart;
    private PartConstruction[] relevantConstructions;
    private List<PlayerConstructShapeUI> shapes = new();

    public void Init(PlayerConstructController player)
    {
        player.OnAvailableConstructionsChange += OnAvailableConstructionsChange;

        // Ensure it is empty on start
        foreach (Transform child in shapesParent.transform) Destroy(child.gameObject);
    }

    public void SetTarget(ConstructPart targetPart = null)
    {
        this.targetPart = targetPart;
        gameObject.SetActive(targetPart != null);

        // Delete shapes on disable
        if (targetPart == null)
        {
            foreach (PlayerConstructShapeUI shapeUI in shapes) Destroy(shapeUI.gameObject);
            shapes.Clear();

            // The layout seems to not update when they are deleted and throws an error on reselect
            shapesLayout.CalculateLayoutInputHorizontal();
        }
    }

    private void Update()
    {
        UpdateBillboard();
    }

    private void UpdateBillboard()
    {
        if (targetPart == null) return;

        // Face towards camera
        transform.forward = Camera.main.transform.forward;

        // Position to the right of the part
        constructionPrompt.position = targetPart.transform.position + Camera.main.transform.right * 0.5f;
    }

    private void RedrawShapes()
    {
        // Destroy all old shape UIs
        foreach (PlayerConstructShapeUI shapeUI in shapes) Destroy(shapeUI.gameObject);
        shapes.Clear();

        // Create new shape UIs
        foreach (PartConstruction construction in relevantConstructions)
        {
            PlayerConstructShapeUI shapeUI = PlayerConstructShapeUI.Create(construction.shape, shapesParent.transform);
            shapes.Add(shapeUI);
            shapeUI.SetSuggestedConstruction(construction);
        }
    }

    private void OnAvailableConstructionsChange(PartConstruction[] constructions)
    {
        if (targetPart == null) return;

        // Either we are constructing target part, or it is a part of the shape being constructed
        relevantConstructions = constructions.Where(
            c => c.part == targetPart || c.shape.Parts.Contains(targetPart)
        ).ToArray();

        // If there are constructions available, show the prompt
        constructionPrompt.gameObject.SetActive(relevantConstructions.Length > 0);
        UpdateBillboard();
        RedrawShapes();
    }
}
