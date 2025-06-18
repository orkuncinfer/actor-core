using System.Collections.Generic;
using StatSystem;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectileDamageComponent : ProjectileComponent
{
    [Header("Projectile Damage Settings")]
    float projectileDamage; public float ProjectileDamage { set => projectileDamage = value; }

    [SerializeField] private EventField<HitNotifyArgs> _hitEvent;
    
    public GunData GunData { get; set; }

    public Ability Ability { get; set; }
    
    
    public void ApplyDamage(GameObject other, Collision collision){
        IHealthComponent healthComponent = other.GetComponent<IHealthComponent>();
        healthComponent?.TakeDamage(projectileDamage);

        if (Ability != null)
        {
            int dmg = GunData.Damage;
            
            if (collision.collider.transform.TryGetComponent(out HumanBodyBoneTag boneTag))
            {
                dmg = GunData.GetBoneMappedDamage(boneTag.Bone);
            }

            dmg = Mathf.Abs(dmg);
            dmg *= -1;
            Debug.Log("ProjectileStandardCollisionComponent: ApplyDamage" + other.name);
            Ability.AbilityDefinition.GameplayEffectDefinitions.ForEach(effect =>
            {
                Debug.Log("Applying effect: " + effect.name);
                //Ability.ApplyEffects(other);
            });

            if (other.TryGetComponent(out ActorBase otherActor))
            {
                StatModifier statModifier = new StatModifier();
                statModifier.Source = Ability;
                statModifier.Magnitude = dmg;
                statModifier.Type = ModifierOperationType.Additive;
                otherActor.GetService<Service_GAS>().EffectController.ApplyStatModifierExternal(statModifier,"Health");

                Vector3 direction = collision.contacts[0].point - transform.position;
                HitNotifyArgs hitNotifyArgs = new HitNotifyArgs();
                hitNotifyArgs.Damage = dmg;
                hitNotifyArgs.Position = collision.contacts[0].point;
                hitNotifyArgs.SurfaceNormal = collision.contacts[0].normal;
                hitNotifyArgs.Direction = direction;
                hitNotifyArgs.Collider = collision.collider;
                _hitEvent.Raise(hitNotifyArgs,otherActor);
            }
        }
    }
}
