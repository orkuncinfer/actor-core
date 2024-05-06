using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GOPoolMember : MonoBehaviour
{
    private GameObjectPool _pool;
    private GameObject _prefab;
    public bool ReturnOnDisable;

    private void Start()
    {
        if(_pool == null)
            GOPoolProvider.RegisterNewItemToPool(this, gameObject);
    }

    public void SetPool(GameObjectPool pool, GameObject prefab)
    {
        _pool = pool;
        _prefab = prefab;
    }

    public void ReturnToPool()
    {
        if(_prefab && _pool)
            _pool.ReturnToPool(gameObject, _prefab);
    }

    private void OnDisable()
    {
        if (ReturnOnDisable)
        {
            ReturnToPool();
        }
    }
}