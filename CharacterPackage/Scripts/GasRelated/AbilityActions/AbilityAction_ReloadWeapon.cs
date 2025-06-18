using Animancer;
using UnityEngine;

public class AbilityAction_ReloadWeapon : AbilityAction
{
    private AimIKWeightHandler _weightHandler;
    private Gun _heldGun;
    public override AbilityAction Clone()
    {
        AbilityAction_ReloadWeapon clone = AbilityActionPool<AbilityAction_ReloadWeapon>.Shared.Get();

        return clone;
    }

    public override void Reset()
    {
        base.Reset();
        _weightHandler = null;
        _heldGun = null;
    }

    public override void OnStart()
    {
        base.OnStart();
        _weightHandler = Owner.GetComponentInChildren<AimIKWeightHandler>();

        ItemDefinition weaponItem = Owner.GetEquippedInstance().GetComponent<Equippable>().ItemDefinition;
        ClipTransition reloadClip = weaponItem.GetData<Data_Gun>().ReloadClip;
        ActiveAbility.SetAnimData(reloadClip);
        Owner.GetComponentInChildren<AnimancerController>().PlayClipTransition(ActiveAbility,reloadClip);
        
        _heldGun = Owner.GetEquippedInstance().GetComponent<Gun>();
        _heldGun.Reload();
    }



    public override void OnExit()
    {
        base.OnExit();
    }
}