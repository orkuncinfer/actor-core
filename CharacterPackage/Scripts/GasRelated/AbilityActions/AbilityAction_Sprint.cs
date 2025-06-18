using System.Collections.Generic;
using ECM2;
using UnityEngine;

public class AbilityAction_Sprint : AbilityAction
{
    ActiveAbility _ability;

    private Character _character;
    public override AbilityAction Clone()
    {
        AbilityAction_Sprint clone = AbilityActionPool<AbilityAction_Sprint>.Shared.Get();
        clone.EventName = EventName;
        clone._character = _character;
        return clone;
    }

    public override void OnStart()
    {
        base.OnStart();
        Debug.Log("Sprint started");
    }
    
    public override void OnTick(Actor owner)
    {
        base.OnTick(owner); 
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("Sprint ended");
    }
}