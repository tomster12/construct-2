using System.Collections.Generic;
using UnityEngine;


public class Construct : MonoBehaviour
{
    private ConstructPart corePart;
    private ConstructMovement currentMovement;
    private HashSet<ConstructPart> constructParts = new HashSet<ConstructPart>();
    private HashSet<ConstructMovement> constructMovements = new HashSet<ConstructMovement>();

    public void Move(Vector3 dir) { if (currentMovement != null) Move(dir); }

    public void Aim(Vector3 pos) { if (currentMovement != null) Aim(pos); }

    public void AddPart(ConstructPart part)
    {
        if (part.IsConstructed) throw new System.Exception("Cannot AddPart(part) when part already constructed.");
        if (constructParts.Contains(part)) throw new System.Exception("Cannot AddPart(part), already registered!");
        constructParts.Add(part);
        part.OnJoinConstruct(this);
    }

    public void RemovePart(ConstructPart part)
    {
        if (!constructParts.Contains(part)) throw new System.Exception("Cannot RemovePart(part), not registered!");
        if (!part.IsConstructed) throw new System.Exception("Cannot RemovePart(part) when part not constructed.");
        constructParts.Remove(part);
        part.OnleaveConstruct();
    }

    public void RegisterPartMovement(ConstructMovement movement)
    {

        if (constructMovements.Contains(movement)) throw new System.Exception("Cannot RegisterPartMovement(movement), already registered!");
        constructMovements.Add(movement);
        if (currentMovement == null) SetMovement(movement);
    }

    public void DeregisterPartMovement(ConstructMovement movement)
    {
        if (!constructMovements.Contains(movement)) throw new System.Exception("Cannot DeregisterPartMovement(movement), not registered!");
        if (currentMovement == movement) SetMovement(null);
        constructMovements.Remove(movement);
    }

    public void SetCore(ConstructPart corePart)
    {
        if (this.corePart != null) throw new System.Exception("Cannot SetCore() when already have a core.");
        this.corePart = corePart;
        AddPart(this.corePart);
    }

    public void SetMovement(ConstructMovement movement)
    {
        if (movement != null && movement.IsControlling) throw new System.Exception("Cannot SetMovement(movement) on a movement that already IsControlling.");
        if (currentMovement != null) currentMovement.SetControlling(false);
        currentMovement = movement;
        if (currentMovement != null) currentMovement.SetControlling(true);
    }
}
