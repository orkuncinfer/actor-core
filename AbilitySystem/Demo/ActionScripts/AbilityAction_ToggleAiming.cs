using ECM2;
using UnityEngine;

public class AbilityAction_ToggleAiming : AbilityAction
{
    ActiveAbility _ability;

    public bool ToggleAiming;
    public bool BackToDefaultOnExit = true;
    public bool IsInstant;
    private AimIKWeightHandler _weightHandler;
    
    private bool _initialAimingState;
    public override AbilityAction Clone()
    {
        AbilityAction_ToggleAiming clone = AbilityActionPool<AbilityAction_ToggleAiming>.Shared.Get();
        clone._weightHandler = _weightHandler;
        clone.ToggleAiming = ToggleAiming;
        clone._initialAimingState = _initialAimingState;
        clone.BackToDefaultOnExit = BackToDefaultOnExit;
        clone.IsInstant = IsInstant;
        return clone;
    }

    public override void OnStart(Actor owner, ActiveAbility ability)
    {
        base.OnStart(owner, ability);
        _weightHandler = owner.GetComponentInChildren<AimIKWeightHandler>();
        if (_weightHandler != null)
        {
            _initialAimingState = _weightHandler.IsAiming;
           _weightHandler.ToggleAiming(ToggleAiming, IsInstant);
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        if (_weightHandler != null && BackToDefaultOnExit)
        {
            _weightHandler.ToggleAiming(_initialAimingState, IsInstant);
        }
    }
}