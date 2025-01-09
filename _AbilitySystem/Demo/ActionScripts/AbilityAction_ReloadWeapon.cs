using Animancer;
using UnityEngine;

public class AbilityAction_ReloadWeapon : AbilityAction
{
    ActiveAbility _ability;
    
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

    public override void OnStart(Actor owner, ActiveAbility ability)
    {
        base.OnStart(owner, ability);
        _weightHandler = owner.GetComponentInChildren<AimIKWeightHandler>();

        ItemDefinition weaponItem = owner.GetEquippedInstance().GetComponent<Equippable>().ItemDefinition;
        ClipTransition reloadClip = weaponItem.GetData<Data_Gun>().ReloadClip;
        ability.SetAnimData(reloadClip);
        owner.GetComponentInChildren<AnimancerController>().PlayClipTransition(ability,reloadClip);
        
        _heldGun = owner.GetEquippedInstance().GetComponent<Gun>();
        _heldGun.Reload();
    }



    public override void OnExit()
    {
        base.OnExit();
    }
}