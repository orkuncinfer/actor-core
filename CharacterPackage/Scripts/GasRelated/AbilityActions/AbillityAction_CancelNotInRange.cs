using UnityEngine;

public class AbillityAction_CancelNotInRange : AbilityAction
{
    private AbilityController _abilityController;
    
    private ActiveAbilityDefinition _abilityDefinition;
    public override AbilityAction Clone()
    {
        base.Clone();
        AbillityAction_CancelNotInRange clone = AbilityActionPool<AbillityAction_CancelNotInRange>.Shared.Get();
        clone.EventName = EventName;
        clone.AnimWindow = AnimWindow;
        clone.ActivationPolicy = ActivationPolicy;

        clone._hasTick = true;
        Debug.Log("Cloned action");
        return clone;
    }

    public override void OnStart()
    {
        base.OnStart();
        _abilityDefinition = Definition as ActiveAbilityDefinition;
        _abilityController = Owner.GetService<Service_GAS>().AbilityController;
        Debug.Log("OnStart called " + Owner);
    }

    public override void OnTick(Actor owner)
    {
        base.OnTick(owner);
     
        if (_abilityController.Target == null)
        {
            RequestEndAbility();
            return;
        }

        float distance = Vector3.Distance(_abilityController.Target.transform.position, Owner.transform.position);
        if (distance > _abilityDefinition.AbilityRange)
        {
            RequestEndAbility();
        }
    }
}
