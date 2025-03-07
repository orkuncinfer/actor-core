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

    public OnGunAction onStopFire;

    public OnGunAction onStartFire;

    private Ability _ability;
    private Gun _gun;

    private float _spreadValue;

    protected override void Start(){
        base.Start();
        _gun = GetComponent<Gun>();
        cam = Camera.main;
        masksToIgnore = ~(1 << 9 |1 << 10); // ignore player, weapons and projectile layers
    }

    protected override void Update()
    {
        base.Update();
        _spreadValue = (_gun.GunData.SpreadRadius.Value * _gun.GunData.SpreadRadiusMultiplier.Value) * projectileSpreadPercentage;
    }

    public override void Perform(Gun gun, GunData gunData,Ability ability = null){
        if (cooldown.IsCooldown) return;

        cooldown.StartCooldownTimer(60 / (gunData.RoundsPerMinute * gunData.RoundsPerMinuteMultiplier.Value));
        _ability = ability;
        _gun = gun;
        Vector3 muzzlePosition = gun.GunMuzzlePosition.transform.position;
        DbgDraw.WireSphere(muzzlePosition, Quaternion.identity,Vector3.one * 0.1f, Color.red, 0.1f);
        GameObject projectile = Instantiate(gunData.ProjectilePrefab, muzzlePosition, Quaternion.identity);
    
        SetProjectileDamage(gunData, projectile,ability);
        
        //float spreadValue = (gunData.SpreadRadius.Value * gunData.SpreadRadiusMultiplier.Value) * projectileSpreadPercentage;
        MoveProjectile(projectile, _spreadValue, muzzlePosition);

        animator.SetTrigger("IsFire");
        PlayAudio();
        gun.DecrementMagazine();

        onFire?.Invoke(gun);
    }

    public void StopFire()
    {
        onStopFire?.Invoke(_gun);
    }

    public void StartFire()
    {
        onStartFire?.Invoke(_gun);
    }
    void PlayAudio(){
        if (gunfireAudio == null) return;
        
        audioSource.PlayOneShot(gunfireAudio);
    }

    public float GetSpreadValue()
    {
        return _spreadValue;
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
        Data_Combatable combatable = _ability.Owner.GetData<Data_Combatable>();
        RaycastHit[] _hits = new RaycastHit[5]; // Small buffer to avoid GC
        Vector3 spreadDeviation = Random.insideUnitCircle * spreadRadius;
        Vector3 origin = cam.transform.position;
        // Adjusting spread deviation based on distance
        Vector3 finalDirection = (cam.transform.forward + (spreadDeviation / maxDistance)).normalized;

        if (combatable.ShootRayFromCamera == false)
        {
            origin = _gun.GunMuzzlePosition.transform.position;
            finalDirection = _gun.GunMuzzlePosition.transform.forward;
        }

        // Create the ray manually
        Ray ray = new Ray(origin, finalDirection);

        int hitCount = Physics.RaycastNonAlloc(ray, _hits, maxDistance, LayerMask, QueryTriggerInteraction.Collide);

        if (hitCount > 0)
        {
            RaycastHit closestHit = default;
            bool foundValidHit = false;
            float minDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                if (((1 << _hits[i].collider.gameObject.layer) & LayerMask) != 0) // Check if hit is in the LayerMask
                {
                    if (_hits[i].distance < minDistance)
                    {
                        minDistance = _hits[i].distance;
                        closestHit = _hits[i];
                        foundValidHit = true;
                    }
                }
            }

            if (foundValidHit)
            {
                //Debug.Log($"Collided layer: {LayerMask.LayerToName(closestHit.collider.gameObject.layer)} collider name {closestHit.collider.name}");
                return closestHit.point;
            }
        }

        return ray.GetPoint(maxDistance);
    }


}
