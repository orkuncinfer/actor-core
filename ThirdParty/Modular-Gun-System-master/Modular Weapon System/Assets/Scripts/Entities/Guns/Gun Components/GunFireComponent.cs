using System;
using Oddworm.Framework;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class GunFireComponent : GunComponent
{
    Camera cam;
    int masksToIgnore;

    public LayerMask LayerMask;
    
    float projectileSpreadPercentage = 1f;
    public float ProjectileSpreadPercentage{
        get => projectileSpreadPercentage; 
        set => projectileSpreadPercentage = Mathf.Clamp(value, 0, 1);
    }
    
    [SerializeField] protected AudioClip gunfireAudio;

    [Header("Fire Delegates")]
    public OnGunAction onFire;

    protected override void Start(){
        base.Start();
        
        cam = Camera.main;
        masksToIgnore = ~(1 << 9 |1 << 10); // ignore player, weapons and projectile layers
    }

    public override void Perform(Gun gun, GunData gunData,Ability ability = null){
        if (cooldown.IsCooldown) return;

        cooldown.StartCooldownTimer(60 / (gunData.RoundsPerMinute * gunData.RoundsPerMinuteMultiplier.Value));

        Vector3 muzzlePosition = gun.GunMuzzlePosition.transform.position;
        DbgDraw.WireSphere(muzzlePosition, Quaternion.identity,Vector3.one * 0.1f, Color.red, 0.1f);
        GameObject projectile = Instantiate(gunData.ProjectilePrefab, muzzlePosition, Quaternion.identity);
    
        SetProjectileDamage(gunData, projectile,ability);
        
        float spreadValue = (gunData.SpreadRadius.Value * gunData.SpreadRadiusMultiplier.Value) * projectileSpreadPercentage;
        MoveProjectile(projectile, spreadValue, muzzlePosition);

        animator.SetTrigger("IsFire");
        PlayAudio();
        gun.DecrementMagazine();

        onFire?.Invoke(gun);
    }
    
    void PlayAudio(){
        if (gunfireAudio == null) return;
        
        audioSource.PlayOneShot(gunfireAudio);
    }

    static bool IsCrit(GunData gunData){
        int critChance = gunData.CritChance.Value;
        return Random.Range(1, 100) <= critChance;
    }
    
    void SetProjectileDamage(GunData gunData, GameObject projectile, Ability ability = null){
        ProjectileDamageComponent projectileDamageComponent = projectile.GetComponent<ProjectileDamageComponent>();
        
        
        float damage = gunData.Damage * gunData.DamageMultiplier.Value;
        bool isCrit = IsCrit(gunData);
        projectileDamageComponent.ProjectileDamage = (isCrit ? gunData.CritMultiplier.Value : 1) * damage;
        projectileDamageComponent.Ability = ability;
        projectileDamageComponent.GunData = gunData;
    }

    void MoveProjectile(GameObject projectile, float spreadRadius, Vector3 muzzlePosition){
        ProjectileTypeComponent typeComponent = projectile.GetComponent<ProjectileTypeComponent>();
        float maxProjectileDistance = typeComponent.MaxProjectileTravel;

        Vector3 projectileDir = GetProjectileDir(spreadRadius, maxProjectileDistance);
        DbgDraw.Line(projectileDir,muzzlePosition,Color.magenta,3);
        // raycast for point of impact is cast from center of camera whereas ballistics travel from the muzzle position
        // and must be offset, this is not required for raycast type as their position is set to point of impact
        if (typeComponent is ProjectileMoveComponent){
            projectileDir -= muzzlePosition;
        }
        typeComponent.InitialiseMovement(projectileDir,muzzlePosition);
    }

    Vector3 GetProjectileDir(float spreadRadius, float maxDistance)
    {
        RaycastHit[] _hits = new RaycastHit[5]; // Allocate a small buffer to reduce allocations
        Vector3 spreadDeviation = Random.insideUnitCircle * spreadRadius;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, cam.nearClipPlane) + (spreadDeviation / maxDistance));

        int hitCount = Physics.RaycastNonAlloc(ray, _hits, maxDistance, LayerMask, QueryTriggerInteraction.Collide);
    
        if (hitCount > 0)
        {
            for (int i = 0; i < hitCount; i++)
            {
                if (((1 << _hits[i].collider.gameObject.layer) & LayerMask) != 0) // Check if hit is in the LayerMask
                {
                    Debug.Log($"Collided layer: {LayerMask.LayerToName(_hits[i].collider.gameObject.layer)}");
                    return _hits[i].point;
                }
            }
        }

        return ray.GetPoint(maxDistance);
    }

}
