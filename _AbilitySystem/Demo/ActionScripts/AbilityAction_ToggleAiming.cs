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

    public override void OnStart(Actor owner, ActiveAbility ability)
    {
        base.OnStart(owner, ability);
        _weightHandler = owner.GetComponentInChildren<AimIKWeightHandler>();
        _ik = owner.GetComponentInChildren<FullBodyBipedIK>();
        if (_weightHandler != null)
        {
            _initialAimingState = _weightHandler.IsAiming;
           //_weightHandler.ToggleAiming(ToggleAiming, IsInstant);
        }

        if (ReleaseLeftHand)
        {
            //_weightHandler.LeftHandPoserToggle(false); why?
            _ik.solver.leftHandEffector.positionWeight = 0;
        }
           
    }

    public override void OnExit()
    {
        base.OnExit();
        if (_weightHandler != null && BackToDefaultOnExit)
        {
            //_weightHandler.ToggleAiming(_initialAimingState, IsInstant);
        }

        if (HoldLeftHandOnExit)
        {
            //_weightHandler.LeftHandPoserToggle(true); why? try to discard this
        }
    }
}