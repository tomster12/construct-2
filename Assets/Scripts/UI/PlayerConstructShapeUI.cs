using UnityEngine;
using UnityEngine.UI;

public class PlayerConstructShapeUI : MonoBehaviour
{
    public static PlayerConstructShapeUI Create(ConstructShape shape, Transform parent)
    {
        string prefabName = "Player Construct Shape UI";
        if (shape is AttachmentShape)
        {
            prefabName += " (AttachmentShape)";
        }
        else
        {
            Debug.LogError("Unknown shape type: " + shape.GetType());
            return null;
        }

        GameObject shapeObject = Instantiate(AssetManager.GetPrefab(prefabName), parent);
        PlayerConstructShapeUI shapeUI = shapeObject.GetComponent<PlayerConstructShapeUI>();
        shapeUI.Init(shape);
        return shapeUI;
    }

    public void Init(ConstructShape shape)
    {
        this.shape = shape;
        this.shape.OnPartsChange += UpdateSlotColours;
        UpdateSlotColours();
        parent.sizeDelta = sizeNotSuggestion * Vector2.one;
    }

    public void UpdateSlotColours()
    {
        // Colour each slot based on whether it's enabled, disabled, or highlighted
        for (int i = 0; i < slotImages.Length; i++)
        {
            slotImages[i].color = shape.GetSlot(i) != null ? colourEnabled
                : (suggestedConstruction != null && suggestedConstruction.Value.slot == i) ? colourSuggesting
                : colourDisabled;
        }
    }

    public void SetSuggestedConstruction(Construction construction)
    {
        // There is a suggested construction for this shape from the player
        suggestedConstruction = construction;
        UpdateSlotColours();
    }

    public void SetIsSuggestion(bool isSuggestion)
    {
        // This is called if this shape UI is a suggestion and not ground truth
        if (isSuggestion == this.isSuggestion) return;
        this.isSuggestion = isSuggestion;

        suggestionBG.gameObject.SetActive(this.isSuggestion);
        suggestionPlus.gameObject.SetActive(this.isSuggestion);

        // We want the shape UI to be irrelevant of the outline size itself
        parent.sizeDelta = (this.isSuggestion ? sizeSuggestion : sizeNotSuggestion) * Vector2.one;
    }

    [Header("References")]
    [SerializeField] private Image[] slotImages;
    [SerializeField] private RectTransform parent;
    [SerializeField] private RectTransform suggestionBG;
    [SerializeField] private RectTransform suggestionPlus;

    [Header("Config")]
    [SerializeField] private float sizeSuggestion = 38.0f;
    [SerializeField] private float sizeNotSuggestion = 35.0f;
    [SerializeField] private Color colourDisabled = new(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color colourEnabled = new(1.0f, 1.0f, 1.0f);
    [SerializeField] private Color colourSuggesting = new(0.4f, 0.6f, 0.93f);

    private ConstructShape shape;
    private Construction? suggestedConstruction;
    private bool isSuggestion = false;

    private void OnDestroy()
    {
        shape.OnPartsChange -= UpdateSlotColours;
        shape = null;
    }
}
