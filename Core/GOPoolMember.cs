using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GOPoolMember : MonoBehaviour
{
    private ObjectPool _pool;
    private GameObject _prefab;
    public bool ReturnOnDisable;
    
    public event Action onBeforeReturnToPool;

    private void Start()
    {
        if(_pool == null)
            PoolProvider.RegisterNewItemToPool(this, gameObject);
    }

    public void SetPool(ObjectPool pool, GameObject prefab)
    {
        _pool = pool;
        _prefab = prefab;
    }

    public void ReturnToPool()
    {
        if (_prefab && _pool)
        {
            onBeforeReturnToPool?.Invoke();
            _pool.ReturnToPool(gameObject, _prefab);
        }
    }

    private void OnDisable()
    {
        if (ReturnOnDisable)
        {
            ReturnToPool();
        }
    }
}