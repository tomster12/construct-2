using UnityEngine;

public class ConstructPartIndicatorUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRendererFG;
    [SerializeField] private SpriteRenderer spriteRendererBG;

    [Header("Config")]
    [SerializeField] private float heightOffset = 0.5f;
    [SerializeField] private Color colourFGHighlighted = new(1.0f, 1.0f, 1.0f, 0.7f);
    [SerializeField] private Color colourFGUnhighlighted = new(1.0f, 1.0f, 1.0f, 0.7f);
    [SerializeField] private Color colourBGHighlighted = new(1.0f, 1.0f, 1.0f, 0.7f);
    [SerializeField] private Color colourBGUnhighlighted = new(1.0f, 1.0f, 1.0f, 0.7f);
    [SerializeField] private float colourLerp = 0.1f;
    [SerializeField] private float sizeHighlighted = 0.5f;
    [SerializeField] private float sizeUnhighlighted = 0.5f;
    [SerializeField] private float sizeLerp = 0.1f;
    [SerializeField] private float heightOscMag = 0.1f;
    [SerializeField] private float heightOscFreq = 1.5f;

    private ConstructPart part;
    private bool isHighlighted = false;
    private float heightOscStart;

    public void Init(ConstructPart part)
    {
        this.part = part;
        SetHighlighted(false);
        UpdateBillboard();
        heightOscStart = Time.time;
    }

    public void SetHighlighted(bool isHighlighted)
    {
        if (isHighlighted == this.isHighlighted) return;
        this.isHighlighted = isHighlighted;
    }

    private void Update()
    {
        UpdateBillboard();
    }

    private void UpdateBillboard()
    {
        if (part == null) return;

        // Face towards camera
        //Vector3 target = Camera.main.transform.position;
        //transform.forward = transform.position - target;
        transform.forward = Camera.main.transform.forward;

        // Oscillate height
        float t = Time.time - heightOscStart;
        float offset = heightOffset + Mathf.Sin(t * heightOscFreq * (Mathf.PI * 2.0f)) * heightOscMag;

        // Set position relative to part
        transform.position = part.transform.position + part.transform.up * offset;

        // Lerp alpha
        Color targetColourFG = isHighlighted ? colourFGHighlighted : colourFGUnhighlighted;
        Color targetColourBG = isHighlighted ? colourBGHighlighted : colourBGUnhighlighted;
        spriteRendererFG.color = Color.Lerp(spriteRendererFG.color, targetColourFG, colourLerp * Time.deltaTime);
        spriteRendererBG.color = Color.Lerp(spriteRendererBG.color, targetColourBG, colourLerp * Time.deltaTime);

        // Lerp size
        float targetSize = isHighlighted ? sizeHighlighted : sizeUnhighlighted;
        float lerpSize = Mathf.Lerp(transform.localScale.x, targetSize, sizeLerp * Time.deltaTime);
        transform.localScale = new Vector3(lerpSize, lerpSize, 1.0f);
    }
}
