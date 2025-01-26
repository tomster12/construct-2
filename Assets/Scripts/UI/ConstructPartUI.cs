using UnityEngine;

public class ConstructPartUI : MonoBehaviour
{
    public void Init(ConstructPart part)
    {
        this.part = part;
        part.OnPropertiesChange += Redraw;
        Redraw();
    }

    [Header("References")]
    [SerializeField] private TMPro.TextMeshProUGUI levelText;
    [SerializeField] private TMPro.TextMeshProUGUI nameText;
    [SerializeField] private GameObject shapesParent;

    private ConstructPart part;

    private void Redraw()
    {
        // Set quick properties
        levelText.text = part.Level.ToString();
        nameText.text = part.gameObject.name;
        //xpGaugeImage.fillAmount = part.XP / part.RequiredXP;

        // Handle updating of shapes
        shapesParent.SetActive(part.Shapes.Count > 0);
        foreach (Transform child in shapesParent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (ConstructShape shape in part.Shapes)
        {
            ConstructShapeUI shapeUI = ConstructShapeUI.Create(shape, shapesParent.transform);
        }
    }

    private void OnDestroy()
    {
        part.OnPropertiesChange -= Redraw;
    }
}
