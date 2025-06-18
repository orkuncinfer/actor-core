using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponForceAimAbility : MonoBehaviour
{
    private Equippable _weapon;
    private ActorBase _actor;

    public AbilityDefinition AimingAbility;

    private bool _equipped;
    private void OnEnable()
    {
        _weapon = GetComponent<Equippable>();
        if(_weapon.IsEquipped) _equipped = true;
        _weapon.onEquipped += OnEquip;
        _weapon.onUnequipped += OnUnequip;
    }
    private void OnDisable()
    {
        _weapon.onUnequipped -= OnUnequip;
        _weapon.onEquipped -= OnEquip;
    }

    private void OnEquip(ActorBase obj)
    {
        _equipped = true;
    }


    private void OnUnequip(ActorBase obj)
    {
        _equipped = false;
        
        _weapon.Owner.GetService<Service_GAS>().AbilityController.CancelAbilityIfActive(AimingAbility.name);
    }

    private void Update()
    {
        if(_weapon == null) return;
        if(!_equipped) return;
        _weapon.Owner.GetService<Service_GAS>().AbilityController.AddAndTryActivateAbility(AimingAbility);
    }
}
