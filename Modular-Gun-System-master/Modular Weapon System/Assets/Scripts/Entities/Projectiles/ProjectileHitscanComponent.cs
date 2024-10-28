using UnityEngine;

[DisallowMultipleComponent]
public class ProjectileHitscanComponent : ProjectileTypeComponent
{
    [SerializeField] private GameObject _tracerPrefab;
    public override void InitialiseMovement(Vector3 pointOfImpact, Vector3 muzzlePosition){
        transform.position = pointOfImpact;
        Debug.Log("Hitscan projectile hit at: " + pointOfImpact);
        GameObject tracerInstance = Instantiate(_tracerPrefab, muzzlePosition, Quaternion.identity);
        TrailRenderer lineRenderer = tracerInstance.GetComponent<TrailRenderer>();
        lineRenderer.AddPosition(muzzlePosition);
        lineRenderer.transform.position = pointOfImpact;
        Debug.Log("pointOfImpact: " + pointOfImpact + " muzzlePosition: " + muzzlePosition);
    }
    
    // Used by gun creation tool for setting up initial values
    public void SetUpProjectileHitscanComponent(float maxProjectileTravel){
        this.maxProjectileTravel = maxProjectileTravel;
    }
}
