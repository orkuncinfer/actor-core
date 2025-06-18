using UnityEngine;

public abstract class ProjectileComponent : MonoBehaviour
{
    [Header("Projectile Effect Settings")]
    [SerializeField] protected bool hasEffect = false;
    [SerializeField] GameObject effect = null;

    protected void InitialiseEffect(Vector3 position, Collision collision = null){
        if (hasEffect)
        {
            GameObject instance = Instantiate(effect, position, Quaternion.identity);
            if (collision != null)
            {
                instance.transform.forward = collision.contacts[0].normal;
            }
        }
    }
}
