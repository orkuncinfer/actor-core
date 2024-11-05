using RootMotion.FinalIK;
using UnityEngine;

public class AbilityAction_ShootingWeapon : AbilityAction
{
    private enum EShootType
    {
        Automatic,
        Single
    }
    [SerializeField] private EShootType _shootType;
    private AimIKWeightHandler _weightHandler;
    private Gun _heldGun;
    public override AbilityAction Clone()
    {
        AbilityAction_ShootingWeapon clone = AbilityActionPool<AbilityAction_ShootingWeapon>.Shared.Get();
        clone._shootType = _shootType;
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
                if(_shootType == EShootType.Single)
                    RequestEndAbility();
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        LastStaticUpdater.onLateUpdate -= OnLateUpdate;
    }
}