using UnityEngine;

[DisallowMultipleComponent]
public class ProjectileHitscanComponent : ProjectileTypeComponent
{
    [SerializeField] private GameObject _tracerPrefab;
    public override void InitialiseMovement(Vector3 pointOfImpact, Vector3 muzzlePosition){
        transform.position = pointOfImpact;
        GameObject tracerInstance = Instantiate(_tracerPrefab, muzzlePosition, Quaternion.identity);
        TrailRenderer lineRenderer = tracerInstance.GetComponent<TrailRenderer>();
        lineRenderer.AddPosition(muzzlePosition);
        lineRenderer.transform.position = pointOfImpact;
    }
    
    // Used by gun creation tool for setting up initial values
    public void SetUpProjectileHitscanComponent(float maxProjectileTravel){
        this.maxProjectileTravel = maxProjectileTravel;
    }
}
