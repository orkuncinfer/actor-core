using RootMotion.FinalIK;

public class AbilityAction_ShootingWeapon : AbilityAction
{
    private AimIKWeightHandler _weightHandler;
    private Gun _heldGun;
    public override AbilityAction Clone()
    {
        AbilityAction_ShootingWeapon clone = AbilityActionPool<AbilityAction_ShootingWeapon>.Shared.Get();

        return clone;
    }

    public override void Reset()
    {
        base.Reset();
        _weightHandler = null;
        _heldGun = null;
    }

    public override void OnStart(Actor owner, ActiveAbility ability)
    {
        base.OnStart(owner, ability);
        _weightHandler = owner.GetComponentInChildren<AimIKWeightHandler>();
    
        _heldGun = owner.GetEquippedInstance().GetComponent<Gun>();
        LastStaticUpdater.onLateUpdate += OnLateUpdate;
    }

    private void OnLateUpdate()
    {
        if(_weightHandler != null)
        {
            _weightHandler.IsAiming = true;
            if (_weightHandler.AimIKWeight >= 1)
            {
                _heldGun.Fire(ActiveAbility);
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        LastStaticUpdater.onLateUpdate -= OnLateUpdate;
    }
}