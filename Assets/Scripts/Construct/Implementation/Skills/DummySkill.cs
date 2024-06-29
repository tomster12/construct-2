using UnityEngine;

public class DummySkill : ConstructSkill
{
    public override string ActionName => "DummySkill";

    public override ActionUseType UseType => ActionUseType.SINGLE;

    public override bool IsActive => false;

    public override bool IsCooldown => false;

    public override void InputDown()
    {
        Debug.Log("DummySkill InputDown");
    }
}
