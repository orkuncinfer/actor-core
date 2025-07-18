using ECM2;
using StatSystem;
using UnityEngine;

public class AbilityAction_CancelAllAbilities : AbilityAction
{
    
    private AbilityController _abilityController;
    
    public override AbilityAction Clone()
    {
        base.Clone();
        AbilityAction_CancelAllAbilities clone = AbilityActionPool<AbilityAction_CancelAllAbilities>.Shared.Get();
        clone.EventName = EventName;
        clone.AnimWindow = AnimWindow;
        
        return clone;
    }

    public override void OnStart()
    {
        base.OnStart();

        _abilityController = Owner.GetService<Service_GAS>().AbilityController;
        

        var activeAbilities = _abilityController.GetActiveAbilities();

        //reverse for
        for (int i = activeAbilities.Count - 1; i >= 0; i--)
        {
            if(activeAbilities[i] == ActiveAbility) continue;
            var ability = activeAbilities[i];
            if (ability != null)
            {
                _abilityController.CancelAbilityIfActive(ability);
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();

        AbilityActionPool<AbilityAction_CancelAllAbilities>.Shared.Release(this);
    }
}