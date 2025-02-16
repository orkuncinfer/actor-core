using ECM2;
using UnityEngine;

public class AbilityAction_Dying : AbilityAction
{
    

    public override AbilityAction Clone()
    {
        AbilityAction_Dying clone = AbilityActionPool<AbilityAction_Dying>.Shared.Get();
        
        return clone;
    }

    public override void OnStart(Actor owner, ActiveAbility ability)
    {
        base.OnStart(owner, ability);
        Transform skeleton = owner.GetComponentInChildren<Animator>().transform;
        
        
    }
    
    public override void OnTick(Actor owner)
    {
        base.OnTick(owner); 
    }

    public override void OnExit()
    {
        base.OnExit();
  
    }
}