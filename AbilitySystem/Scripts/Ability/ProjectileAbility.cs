using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileAbility : ActiveAbility
{
    public new ProjectileAbilityDefinition definition => _abilityDefinition as ProjectileAbilityDefinition;
    
    public ProjectileAbility(ActiveAbilityDefinition definition, AbilityController controller) : base(definition, controller)
    {
        
    }

    private void OnHit(CollisionData data)
    {
        if (data.source is Projectile projectile)
        {
            projectile.Rigidbody.velocity = Vector3.zero;
            ApplyEffects(data.target);
            projectile.hit -= OnHit;
            PoolManager.ReleaseObject(projectile.gameObject);
        }
    }
    
    public void Shoot(GameObject target)
    {
        GameObject equipment = Owner.GetEquippedInstance();
        if (equipment.TryGetComponent(out RangedWeapon weapon))
        {
            GameObject projectileInstance =
                PoolManager.SpawnObject(definition.projectilePrefab, Vector3.zero, Quaternion.identity);
            Projectile projectile = projectileInstance.GetComponent<Projectile>();

            projectile.hit += OnHit;
            
            weapon.Shoot(
                projectile,
                target.transform,
                definition.projectileSpeed,
                definition.shotType,
                definition.isSpinning);
        }
        else
        {
            Debug.LogWarning($"User does not have a ranged weapon equipped.");
        }
    }
}
