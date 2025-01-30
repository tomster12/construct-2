using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    private static Dictionary<string, Sprite> spriteDict = new();
    private static Dictionary<string, GameObject> prefabDict = new();
    private static Dictionary<string, Material> materialDict = new();
    private static Dictionary<string, Font> fontDict = new();

    [Header("Assets")]
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] materials;
    [SerializeField] private Font[] fonts;

    public static Sprite GetSprite(string name) => spriteDict[name];

    public static GameObject GetPrefab(string name) => prefabDict[name];

    public static Material GetMaterial(string name) => materialDict[name];

    public static Font GetFont(string name) => fontDict[name];

    private void OnValidate()
    {
        spriteDict ??= new Dictionary<string, Sprite>();
        prefabDict ??= new Dictionary<string, GameObject>();
        materialDict ??= new Dictionary<string, Material>();
        fontDict ??= new Dictionary<string, Font>();
        foreach (Sprite sprite in sprites) spriteDict[sprite.name] = sprite;
        foreach (GameObject prefab in prefabs) prefabDict[prefab.name] = prefab;
        foreach (Material material in materials) materialDict[material.name] = material;
        foreach (Font font in fonts) fontDict[font.name] = font;
    }
}
