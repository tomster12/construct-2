using System.Linq;
using UnityEngine;

public class PartViewUI : MonoBehaviour
{
    public void Init(ConstructPart part)
    {
        this.part = part;
        part.OnPropertiesChange += Redraw;
        Redraw();
    }

    [Header("Prefabs")]
    [SerializeField] private GameObject shapePrefab;

    [Header("References")]
    [SerializeField] private TMPro.TextMeshProUGUI levelText;
    [SerializeField] private UnityEngine.UI.Image xpGaugeImage;
    [SerializeField] private UnityEngine.UI.Image iconImage;
    [SerializeField] private TMPro.TextMeshProUGUI nameText;
    [SerializeField] private TMPro.TextMeshProUGUI weightClassText;
    [SerializeField] private GameObject shapesParent;

    private ConstructPart part;
    private PlayerConstructController PlayerConstructController;

    private void Redraw()
    {
        // Set quick properties
        levelText.text = part.Level.ToString();
        iconImage.sprite = part.Icon;
        nameText.text = part.gameObject.name;
        weightClassText.text = "(" + part.WeightClass.ToString() + ")";
        xpGaugeImage.fillAmount = part.XP / part.RequiredXP;

        // Handle updating of shapes

        shapesParent.SetActive(part.Shapes.Count > 0);

        foreach (Transform child in shapesParent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (ConstructShape shape in part.Shapes)
        {
            ShapeUI shapeUI = ShapeUIFactory.Create(shape, shapesParent.transform);
        }
    }

    private void OnDestroy()
    {
        part.OnPropertiesChange -= Redraw;
    }
}
