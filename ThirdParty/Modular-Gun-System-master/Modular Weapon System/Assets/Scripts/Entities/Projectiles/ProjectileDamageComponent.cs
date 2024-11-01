using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectileDamageComponent : ProjectileComponent
{
    [Header("Projectile Damage Settings")]
    float projectileDamage; public float ProjectileDamage { set => projectileDamage = value; }

    public Ability Ability { get; set; }
    
    public void ApplyDamage(GameObject other){
        IHealthComponent healthComponent = other.GetComponent<IHealthComponent>();
        healthComponent?.TakeDamage(projectileDamage);

        if (Ability != null)
        {
            Debug.Log("ProjectileStandardCollisionComponent: ApplyDamage" + other.name);
            Ability.AbilityDefinition.GameplayEffectDefinitions.ForEach(effect =>
            {
                Debug.Log("Applying effect: " + effect.name);
                Ability.ApplyEffects(other);
            });
        }
    }
}
