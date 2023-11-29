using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] ConstructPart corePart;
    [SerializeField] Construct construct;

    private void Awake()
    {
        construct.SetCore(corePart);
    }
}