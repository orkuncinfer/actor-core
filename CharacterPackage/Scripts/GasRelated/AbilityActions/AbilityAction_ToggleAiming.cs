using ECM2;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using UnityEngine;

public class AbilityAction_ToggleAiming : AbilityAction
{
    ActiveAbility _ability;

    public bool ToggleAiming;
    public bool BackToDefaultOnExit = true;
    public bool IsInstant;
    public bool ReleaseLeftHand;
    [ShowIf("ReleaseLeftHand")]public bool HoldLeftHandOnExit;
    private AimIKWeightHandler _weightHandler;
    private FullBodyBipedIK _ik;
    
    private bool _initialAimingState;
    public override AbilityAction Clone()
    {
        AbilityAction_ToggleAiming clone = AbilityActionPool<AbilityAction_ToggleAiming>.Shared.Get();
        clone._weightHandler = _weightHandler;
        clone.ToggleAiming = ToggleAiming;
        clone._initialAimingState = _initialAimingState;
        clone.BackToDefaultOnExit = BackToDefaultOnExit;
        clone.IsInstant = IsInstant;
        clone.ReleaseLeftHand = ReleaseLeftHand;
        clone.HoldLeftHandOnExit = HoldLeftHandOnExit;
        return clone;
    }
    public override void Reset()
    {
        base.Reset();
        _ik = null;
        _weightHandler = null;
    }
    public override void OnStart()
    {
        base.OnStart();
        _weightHandler = Owner.GetComponentInChildren<AimIKWeightHandler>();
        _ik = Owner.GetComponentInChildren<FullBodyBipedIK>();
        if (_weightHandler != null)
        {
            _initialAimingState = _weightHandler.IsAiming;
        }

        if (ReleaseLeftHand)
        {
            _weightHandler.ReleaseLeftHand(0.5f);
        }
        
    }

    public override void OnExit()
    {
        base.OnExit();
        if (HoldLeftHandOnExit)
        {
            _weightHandler.HoldLeftHand(0.25f);
        }
    }
}