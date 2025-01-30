using UnityEngine;

public class DebugConstructListener : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Construct construct;
    [SerializeField] private DebugLog debugLog;

    private void Awake()
    {
        construct.OnPartEvent += OnConstructPartEvent;
        construct.OnSkillEvent += OnConstructSkillEvent;
        construct.OnMovementEvent += OnConstructMovementEvent;
        construct.OnShapeEvent += OnConstructShapeEvent;
        construct.OnActiveShapeEvent += OnConstructActiveShapeEvent;
    }

    private void OnConstructPartEvent(Construct.EventType eventType, ConstructPart part)
    {
        debugLog.AddMessage($"Construct PartEvent({eventType}, {part.name}) [{construct.gameObject.name}]", DebugLog.Category.Construct);

        if (eventType == Construct.EventType.Add)
        {
            part.OnConstructedChange += OnConstructedChange;
            part.OnActiveShapeEvent += OnPartActiveShapeEvent;
            part.OnMovementEvent += OnPartMovementEvent;
            part.OnSkillEvent += OnPartSkillEvent;
        }
        else if (eventType == Construct.EventType.Remove)
        {
            part.OnConstructedChange -= OnConstructedChange;
            part.OnActiveShapeEvent -= OnPartActiveShapeEvent;
            part.OnMovementEvent -= OnPartMovementEvent;
            part.OnSkillEvent -= OnPartSkillEvent;
        }
    }

    private void OnConstructSkillEvent(Construct.EventType eventType, ConstructSkill skill)
    {
        debugLog.AddMessage($"Construct SkillEvent({eventType}, {skill.name}) [{construct.gameObject.name}]", DebugLog.Category.Construct);
    }

    private void OnConstructMovementEvent(Construct.EventType eventType, ConstructMovement movement)
    {
        debugLog.AddMessage($"Construct MovementEvent({eventType}, {movement.name}) [{construct.gameObject.name}]", DebugLog.Category.Construct);

        if (eventType == Construct.EventType.Add)
        {
            movement.OnStateChange += OnMovementStateChange;
        }
        else if (eventType == Construct.EventType.Remove)
        {
            movement.OnStateChange -= OnMovementStateChange;
        }
    }

    private void OnConstructShapeEvent(Construct.EventType eventType, ConstructShape shape, ConstructPart part)
    {
        debugLog.AddMessage($"Construct ShapeEvent({eventType}, {shape.name}) [{construct.gameObject.name}]", DebugLog.Category.Construct);

        if (eventType == Construct.EventType.Add)
        {
            shape.OnPartsChange += OnShapePartsChange;
        }
        else if (eventType == Construct.EventType.Remove)
        {
            shape.OnPartsChange -= OnShapePartsChange;
        }
    }

    private void OnConstructActiveShapeEvent(Construct.EventType eventType, ConstructShape shape, ConstructPart part)
    {
        debugLog.AddMessage($"Construct ActiveShapeEvent({eventType}, {shape.name}, {part}) [{construct.gameObject.name}]", DebugLog.Category.Construct);
    }

    private void OnConstructedChange(ConstructPart part, ConstructPart.EventType eventType, Construct construct)
    {
        debugLog.AddMessage($"Part ConstructedChange({eventType}, {construct.name}) [{part.name}]", DebugLog.Category.Part);
    }

    private void OnPartSkillEvent(ConstructPart part, ConstructPart.EventType eventType, ConstructSkill skill)
    {
        debugLog.AddMessage($"Part SkillEvent({eventType}, {skill.name}) [{part.name}]", DebugLog.Category.Part);
    }

    private void OnPartMovementEvent(ConstructPart part, ConstructPart.EventType eventType, ConstructMovement movement)
    {
        debugLog.AddMessage($"Part MovementEvent({eventType}, {movement.name}) [{part.name}]", DebugLog.Category.Part);
    }

    private void OnPartActiveShapeEvent(ConstructPart part, ConstructPart.EventType eventType, ConstructShape shape)
    {
        debugLog.AddMessage($"Part ActiveShapeEvent({eventType}, {shape.name}) [{part.name}]", DebugLog.Category.Part);
    }

    private void OnMovementStateChange(ConstructMovement movement, bool state)
    {
        debugLog.AddMessage($"Movement StateChange({state}) [{movement.name}]", DebugLog.Category.Movement);
    }

    private void OnShapePartsChange(ConstructShape shape, bool isAdded)
    {
        debugLog.AddMessage($"Shape PartsChange({isAdded}) [{shape.name}]", DebugLog.Category.Shape);
    }
}
