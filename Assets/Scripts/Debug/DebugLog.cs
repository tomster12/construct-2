using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugLog : MonoBehaviour
{
    public enum Category
    {
        Construct, Part, Skill, Movement, Shape, Info
    }

    [Header("Prefabs")]
    [SerializeField] private GameObject messagePrefab;

    [Header("References")]
    [SerializeField] private GameObject messagesParent;
    [SerializeField] private VerticalLayoutGroup messagesLayout;

    [Header("Config")]
    [SerializeField] private int maxMessages = 10;
    [SerializeField] private Color ColourConstruct = Color.red;
    [SerializeField] private Color ColourPart = Color.green;
    [SerializeField] private Color ColourSkill = Color.yellow;
    [SerializeField] private Color ColourMovement = Color.blue;
    [SerializeField] private Color ColourShape = Color.magenta;
    [SerializeField] private Color ColourInfo = Color.white;

    private List<GameObject> messages = new();

    public void AddMessage(string message, Category category)
    {
        // Instantiate new message
        GameObject newMessage = Instantiate(messagePrefab, messagesParent.transform);
        TMPro.TextMeshProUGUI textUI = newMessage.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        Image image = newMessage.GetComponent<Image>();
        messages.Add(newMessage);

        // Convert category to colour
        Color color = category switch
        {
            Category.Construct => ColourConstruct,
            Category.Part => ColourPart,
            Category.Skill => ColourSkill,
            Category.Movement => ColourMovement,
            Category.Shape => ColourShape,
            Category.Info => ColourInfo,
            _ => Color.white
        };

        // Set values on the message
        textUI.text = message;
        image.color = color;

        // Remove oldest if we have too many
        if (messages.Count > maxMessages)
        {
            Destroy(messages[0]);
            messages.RemoveAt(0);
        }

        // Update the layout
        messagesLayout.CalculateLayoutInputHorizontal(); // ?
        messagesLayout.CalculateLayoutInputVertical();
    }

    [ContextMenu("Clear Messages")]
    public void ClearMessages()
    {
        foreach (GameObject message in messages)
        {
            Destroy(message);
        }
        messages.Clear();
    }
}
