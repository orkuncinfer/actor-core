using DG.Tweening;
using RootMotion.FinalIK;
using UnityEngine;
[System.Serializable]
public class AbilityAction_CancelAbilityAfterTime : AbilityAction
{

    public float Time;
    private AbilityController _abilityController;

    public override AbilityAction Clone()
    {
        AbilityAction_CancelAbilityAfterTime clone = AbilityActionPool<AbilityAction_CancelAbilityAfterTime>.Shared.Get();
        clone.Time = Time;
        return clone;
    }

    public override void OnStart()
    {
        base.OnStart();
        _abilityController = Owner.GetComponentInChildren<AbilityController>();
      
        DOVirtual.DelayedCall(Time, () =>
        {
            _abilityController.CancelAbilityIfActive(Definition.name);
        });
    }

  
}