using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackAction : AbilityAction
{
    private MeleeWeapon _meleeWeapon;
    ActiveAbility _ability;
    private List<Collider> _appliedColliders = new List<Collider>();

    public override void OnStart(Actor owner, ActiveAbility ability)
    {
        base.OnStart(owner, ability);
        if (owner.GetEquippedInstance().TryGetComponent(out MeleeWeapon weapon))
        {
            _meleeWeapon = weapon;
            _meleeWeapon.onHit += OnHit;
            _ability = ability;
        }
    }

    private void OnHit(Collider obj)
    {
        if (_appliedColliders.Contains(obj))
            return;
        _ability.AbilityDefinition.GameplayEffectDefinitions.ForEach(effect =>
        {
            _ability.ApplyEffects(obj.gameObject);
            _appliedColliders.Add(obj);
        });
    }

    public override void OnTick(Actor owner)
    {
        base.OnTick(owner);
        if(_meleeWeapon)
            _meleeWeapon.Cast();
    }

    public override void OnExit(Actor owner)
    {
        base.OnExit(owner);
        if (_meleeWeapon)
        {
            _meleeWeapon.onHit -= OnHit;
        }
        _appliedColliders.Clear();
    }
}
