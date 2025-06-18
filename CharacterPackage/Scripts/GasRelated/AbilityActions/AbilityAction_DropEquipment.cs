using UnityEngine;

public class AbilityAction_DropEquipment : AbilityAction
{
    public override AbilityAction Clone()
    {
        AbilityAction_DropEquipment clone = AbilityActionPool<AbilityAction_DropEquipment>.Shared.Get();
        return clone;
    }

    public override void Reset()
    {
        base.Reset();
    }

    public override void OnStart()
    {
        base.OnStart();
        Owner.GetData<DS_EquipmentUser>().DropCurrent();
        
        RequestEndAbility();
    }
}