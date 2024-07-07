using System;
using UnityEngine;
using System.Collections.Generic;

public static class PoolProvider
{
    private static ObjectPool _pool;

    public static void RegisterNewItemToPool(GOPoolMember member, GameObject prefab)
    {
        if (_pool == null)
        {
            GameObject poolObject = new GameObject("Global Object Pool");
            _pool = poolObject.AddComponent<ObjectPool>();
        }
        member.SetPool(_pool, prefab);
    }
    public static GameObject Retrieve(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (_pool == null)
        {
            GameObject poolObject = new GameObject("Global Object Pool");
            _pool = poolObject.AddComponent<ObjectPool>();
        }

        return _pool.RetrieveFromPool(prefab, position, rotation, parent);
    }
    
    public static object Retrieve<T>() where T : new()
    {
        if (_pool == null)
        {
            GameObject poolObject = new GameObject("Global Object Pool");
            _pool = poolObject.AddComponent<ObjectPool>();
        }

        return _pool.RetrieveFromPool<T>();
    }

    public static void ReturnObjectToPool(object obj)
    {
        _pool.ReturnToPool(obj);
    }
}