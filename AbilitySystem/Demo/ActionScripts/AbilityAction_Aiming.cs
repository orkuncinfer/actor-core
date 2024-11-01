using RootMotion.FinalIK;

public class AbilityAction_Aiming : AbilityAction
{
    ActiveAbility _ability;

    private AimIKWeightHandler _weightHandler;
    private FullBodyBipedIK _ik;
    
    private bool _initialAimingState;
    public override AbilityAction Clone()
    {
        AbilityAction_Aiming clone = AbilityActionPool<AbilityAction_Aiming>.Shared.Get();

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

        _ik.solver.leftHandEffector.positionWeight = 1;
    
        StaticUpdater.onUpdate += OnUpdate;
    }

    private void OnUpdate()
    {
        if(_weightHandler != null)
        {
            _weightHandler.IsAiming = true;
        }
    }

    public override void OnExit()
    {
        _weightHandler.IsAiming = false;
        base.OnExit();
       StaticUpdater.onUpdate -= OnUpdate;
       _weightHandler.IsAiming = false;
    }
}