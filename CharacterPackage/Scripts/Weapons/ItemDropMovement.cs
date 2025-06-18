using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemDropMovement : MonoBehaviour
{
    private Equippable _equippable;

    private Rigidbody _rigidbody;

    private bool _dropped;

    private void Start()
    {
        _equippable = GetComponent<Equippable>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnDestroy()
    {
    }

    private void OnDropped(ActorBase obj)
    {
        _dropped = false;
        _equippable.transform.SetParent(null);
        if (_equippable.TryGetComponent(out Rigidbody rigidbody))
        {
            _rigidbody = rigidbody;
            Physics.IgnoreCollision(_equippable.GetComponent<Collider>(),obj.GetComponent<Collider>());
            _equippable.GetComponent<Collider>().enabled = true;
            rigidbody.isKinematic = false;
            rigidbody.AddForce(obj.transform.forward * 100);
            Vector3 randomTorque = Random.onUnitSphere.normalized * 0.01f;
            rigidbody.AddTorque(randomTorque, ForceMode.Impulse);

            if (obj.TryGetService(out AimIKWeightHandler aimIk))
            {
                aimIk.ReleaseInstant();
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_dropped == false)
        {
            _rigidbody.AddForce(Vector3.up * 100);
            _dropped = true;
        }
        Debug.Log("item drop collision");
    }

    public void Drop()
    {
        
    }
}
