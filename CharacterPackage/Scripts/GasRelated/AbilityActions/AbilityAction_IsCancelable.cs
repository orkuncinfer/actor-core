using ECM2;
using UnityEngine;

public class AbilityAction_IsCancelable : AbilityAction
{
    [SerializeField] private GameplayTagContainer _tagsToAdd;
    
    public override AbilityAction Clone()
    {
        AbilityAction_IsCancelable clone = AbilityActionPool<AbilityAction_IsCancelable>.Shared.Get();
        clone.EventName = EventName;
        clone.AnimWindow = AnimWindow;
        
        clone._tagsToAdd = _tagsToAdd;
        return clone;
    }

    public override void OnStart()
    {
        base.OnStart();
        ActiveAbility.CanBeCanceled = true;
        //Owner.GetService<Service_GAS>().AbilityController
    }
    
    public override void OnTick(Actor owner)
    {
        base.OnTick(owner); 
    }

    public override void OnExit()
    {
        base.OnExit();
        ActiveAbility.CanBeCanceled = false;
        AbilityActionPool<AbilityAction_IsCancelable>.Shared.Release(this);
    }
}