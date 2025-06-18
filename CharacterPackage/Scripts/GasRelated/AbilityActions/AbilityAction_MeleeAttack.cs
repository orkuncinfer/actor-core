using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AbilityAction_MeleeAttack : AbilityAction
{
    public GameObject HitEffect;
    public AbilityDefinition OnHitApplyAbility;
    private MeleeWeapon _meleeWeapon;
    private List<Collider> _appliedColliders = new List<Collider>();
    
    public override AbilityAction Clone()
    {
        base.Clone();
        AbilityAction_MeleeAttack clone = AbilityActionPool<AbilityAction_MeleeAttack>.Shared.Get();
        clone.EventName = EventName;
        clone.AnimWindow = AnimWindow;
        
        clone.HitEffect = HitEffect;
        clone._meleeWeapon = _meleeWeapon;
        clone.OnHitApplyAbility = OnHitApplyAbility;
        clone._hasTick = true;
        return clone;
    }

    public override void OnStart()
    {
        base.OnStart();
        
        Debug.Log("melee enter");
        if (Owner.GetEquippedInstance() == null)
        {
            return;
        }
        if (Owner.GetEquippedInstance().TryGetComponent(out MeleeWeapon weapon))
        {
            _meleeWeapon = weapon;
            _meleeWeapon.onHit += OnHit;
            //ActiveAbility = ability; ?
        }
    }
    

    private void OnHit(Collider obj)
    {
        Actor hitActor = obj.GetComponent<Actor>();
        AbilityController abilityController = hitActor.GetService<Service_GAS>().AbilityController;
        if (hitActor.GameplayTags.HasTagExact("State.IsDead"))
        {
            return;
        }
        
        if (_appliedColliders.Contains(obj)) // prevent applying effects to the same collider multiple times
            return;
        
        Owner.GetComponent<Network_HandleDamageNumbers>().RegisterAsTarget(hitActor);
        
        if (OnHitApplyAbility)
        {
            //Debug.Log("tried to apply ability on hit : " + hitActor.name + " with ability : " + OnHitApplyAbility.name);
            abilityController.AddAndTryActivateAbility(OnHitApplyAbility);
        }
        
        ActiveAbility.AbilityDefinition.GameplayEffectDefinitions.ForEach(effect =>
        {
            ActiveAbility.ApplyEffects(obj.gameObject);
            _appliedColliders.Add(obj);
        });
        PoolManager.SpawnObject(HitEffect, obj.ClosestPoint(_meleeWeapon.Collider.center), Quaternion.identity);
    }

    public override void OnTick(Actor owner)
    {
        base.OnTick(owner);
        if(_meleeWeapon)
            _meleeWeapon.Cast();
    }

    public override void OnExit()
    {
        base.OnExit();
        if (_meleeWeapon)
        {
            _meleeWeapon.onHit -= OnHit;
        }
        AbilityActionPool<AbilityAction_MeleeAttack>.Shared.Release(this);
        _appliedColliders.Clear();
    }
}
