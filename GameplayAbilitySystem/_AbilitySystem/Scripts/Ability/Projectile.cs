using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    protected Rigidbody _rigidbody;
    public Rigidbody Rigidbody => _rigidbody;

    protected Collider _collider;

    public event Action<CollisionData> hit;

    [SerializeField] private VisualEffect m_CollisionVisualEffectPrefab;

    protected void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    protected void HandleCollision(Collision collision)
    {
        if (m_CollisionVisualEffectPrefab != null)
        {
            VisualEffect collisionVisualEffect =
                Instantiate(m_CollisionVisualEffectPrefab, transform.position, transform.rotation);

            collisionVisualEffect.transform.forward = collision.GetContact(0).normal;
            collisionVisualEffect.finished += effect => Destroy(effect.gameObject);
            collisionVisualEffect.Play();
        }
        hit?.Invoke(new CollisionData
        {
            source = this,
            target = collision.gameObject
        });
    }
}