using ECM2;
using FIMSpace;
using FIMSpace.FProceduralAnimation;
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

        if (skeleton.TryGetComponent(out LegsAnimator  legsAnimator))
        {
            legsAnimator.enabled = false;
        }
        if (skeleton.TryGetComponent(out LeaningAnimator  leanAnimator))
        {
            leanAnimator.enabled = false;
        }

        owner.GetData<Data_Living>().ShouldDieTrigger = true;
    }
    

    public override void OnExit()
    {
        base.OnExit();
  
    }
}