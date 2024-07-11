using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [ShowInInspector]
    private Dictionary<GameObject, Queue<GameObject>> _gameObjectPoolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    [ShowInInspector]
    private Dictionary<System.Type, Queue<object>> _objectPoolDictionary = new Dictionary<System.Type, Queue<object>>();

    public GameObject RetrieveFromPool(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!_gameObjectPoolDictionary.TryGetValue(prefab, out Queue<GameObject> objectPool))
        {
            objectPool = new Queue<GameObject>();
            _gameObjectPoolDictionary[prefab] = objectPool;
        }

        GameObject obj;
        if (objectPool.Count == 0)
        {
            obj = Instantiate(prefab, position, rotation, parent);
            var poolMember = obj.GetComponent<GOPoolMember>() ?? obj.AddComponent<GOPoolMember>();
            poolMember.SetPool(this, prefab);
        }
        else
        {
            obj = objectPool.Dequeue();
            Debug.Log("Retrieving object from pool " + obj.name);
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
        }

        return obj;
    }

    public void ReturnToPool(GameObject obj, GameObject prefab)
    {
        obj.SetActive(false);
        if (!_gameObjectPoolDictionary.TryGetValue(prefab, out Queue<GameObject> objectPool))
        {
            objectPool = new Queue<GameObject>();
            _gameObjectPoolDictionary[prefab] = objectPool;
        }

        objectPool.Enqueue(obj);
        Debug.Log("Returning object to pool " + obj.name);
    }

    public T RetrieveFromPool<T>() where T : new()
    {
        var type = typeof(T);
        if (!_objectPoolDictionary.TryGetValue(type, out Queue<object> objectPool))
        {
            objectPool = new Queue<object>();
            _objectPoolDictionary[type] = objectPool;
        }

        if (objectPool.Count == 0)
        {
            Debug.Log("Creating new object " + type.Name);
            return new T();
        }
        else
        {
            return (T)objectPool.Dequeue();
        }
    }

    public void ReturnToPool(object obj)
    {
        var type = obj.GetType();
        if (!_objectPoolDictionary.TryGetValue(type, out Queue<object> objectPool))
        {
            objectPool = new Queue<object>();
            _objectPoolDictionary[type] = objectPool;
        }

        objectPool.Enqueue(obj);
    }
}
