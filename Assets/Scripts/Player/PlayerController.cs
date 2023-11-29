using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] ConstructPart corePart;
    [SerializeField] Construct construct;

    private bool isAttaching = false;

    private void Start()
    {
        construct.SetCore(corePart);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttaching)
        {
            construct.UseDownAction("Attach");
            isAttaching = true;
        }
        else if (Input.GetMouseButtonUp(0) && isAttaching)
        {
            construct.UseUpAction("Attach");
            isAttaching = false;
        }
        construct.VisualiseAllActions();
    }
}
