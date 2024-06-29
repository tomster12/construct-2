using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject partViewPrefab;

    [Header("References")]
    [SerializeField] private Construct construct;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private RectTransform constructPartViewsParent;
    [SerializeField] private RectTransform constructPartViewsNoneText;
    [SerializeField] private RectTransform targettedPartViewParent;
    [SerializeField] private RectTransform targettedPartViewNoneText;

    private void Start()
    {
        construct.OnConstructChange += Redraw;
        Redraw();
    }

    private void OnDestroy()
    {
        construct.OnConstructChange -= Redraw;
    }

    private void Redraw()
    {
        RedrawConstructPartViews();
    }

    private void RedrawConstructPartViews()
    {
        // Redraw part view list
        foreach (Transform child in constructPartViewsParent)
        {
            Destroy(child.gameObject);
        }

        constructPartViewsNoneText.gameObject.SetActive(construct.Parts.Count == 0);

        const float HEIGHT = 35.0f;
        const float GAP = 5.0f;
        const float PADDING = 5.0f;
        for (int i = 0; i < construct.Parts.Count; i++)
        {
            ConstructPart part = construct.Parts[i];
            GameObject partViewObject = Instantiate(partViewPrefab, constructPartViewsParent);
            RectTransform rectTfm = partViewObject.GetComponent<RectTransform>();
            PartViewUI partView = partViewObject.GetComponent<PartViewUI>();
            rectTfm.anchoredPosition = new Vector2(PADDING, -i * (HEIGHT + GAP) - PADDING);
            partView.Init(part);
        }
    }
}
