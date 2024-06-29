using UnityEngine;

public class ShapeUIFactory : MonoBehaviour
{
    public static ShapeUI Create(ConstructShape shape, Transform parent)
    {
        if (shape is AttachmentShape)
        {
            GameObject shapeObject = Instantiate(Resources.Load<GameObject>("Attachment Shape UI"), parent);
            AttachmentShapeUI shapeUI = shapeObject.GetComponent<AttachmentShapeUI>();
            shapeUI.Init(shape as AttachmentShape);
            return shapeUI;
        }
        else
        {
            Debug.LogError("Unknown shape type: " + shape.GetType());
            return null;
        }
    }
}
