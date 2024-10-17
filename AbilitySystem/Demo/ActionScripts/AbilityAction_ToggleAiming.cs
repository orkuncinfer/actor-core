using ECM2;
using UnityEngine;

public class AbilityAction_ToggleAiming : AbilityAction
{
    ActiveAbility _ability;

    public bool ToggleAiming;
    public bool BackToDefaultOnExit = true;
    private AimIKWeightHandler _weightHandler;
    
    private bool _initialAimingState;
    public override AbilityAction Clone()
    {
        AbilityAction_ToggleAiming clone = AbilityActionPool<AbilityAction_ToggleAiming>.Shared.Get();
        clone._weightHandler = _weightHandler;
        clone.ToggleAiming = ToggleAiming;
        clone._initialAimingState = _initialAimingState;
        clone.BackToDefaultOnExit = BackToDefaultOnExit;
        return clone;
    }

    public override void OnStart(Actor owner, ActiveAbility ability)
    {
        base.OnStart(owner, ability);
        _weightHandler = owner.GetComponentInChildren<AimIKWeightHandler>();
        if (_weightHandler != null)
        {
            _initialAimingState = _weightHandler.IsAiming;
           _weightHandler.ToggleAiming(ToggleAiming);
        }
        Debug.Log("test341");
    }

    public override void OnExit()
    {
        base.OnExit();
        if (_weightHandler != null && BackToDefaultOnExit)
        {
            _weightHandler.IsAiming = _initialAimingState;
        }
        Debug.Log("test342 end");
    }
}