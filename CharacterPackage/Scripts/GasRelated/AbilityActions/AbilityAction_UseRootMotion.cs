using ECM2;
using UnityEngine;

public class AbilityAction_UseRootMotion : AbilityAction
{
    public bool PauseGroundConstraint = false;
    
    ActiveAbility _ability;

    private Character _character;
    public override AbilityAction Clone()
    {
        AbilityAction_UseRootMotion clone = AbilityActionPool<AbilityAction_UseRootMotion>.Shared.Get();
        clone.EventName = EventName;
        clone._character = _character;
        clone.PauseGroundConstraint =  PauseGroundConstraint;
        return clone;
    }

    public override void OnStart()
    {
        base.OnStart();
        _character = Owner.GetData<Data_Character>().Character;
    
        _character.useRootMotion = true;
        
    }
    
    public override void OnTick(Actor owner)
    {
        base.OnTick(owner); 
    }

    public override void OnExit()
    {
        base.OnExit();
        _character.useRootMotion = false;
        AbilityActionPool<AbilityAction_UseRootMotion>.Shared.Release(this);
    }
}