using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    [ShowInInspector]private Dictionary<GameObject, Queue<GameObject>> _poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    public GameObject RetrieveFromPool(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!_poolDictionary.TryGetValue(prefab, out Queue<GameObject> objectPool))
        {
            objectPool = new Queue<GameObject>();
            _poolDictionary[prefab] = objectPool;
        }

        GameObject obj;
        if (objectPool.Count == 0)
        {
            obj = Instantiate(prefab, position, rotation,parent);
            GOPoolMember poolMember;
            if (obj.transform.TryGetComponent(out GOPoolMember member))
            {
                poolMember = member;
            }
            else
            {
                poolMember = obj.AddComponent<GOPoolMember>();
            }
            poolMember.SetPool(this, prefab);
        }
        else
        {
            obj = objectPool.Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
        }

        return obj;
    }
    

    public void ReturnToPool(GameObject obj, GameObject prefab)
    {
        obj.SetActive(false);
        if (!_poolDictionary.TryGetValue(prefab, out Queue<GameObject> objectPool))
        {
            objectPool = new Queue<GameObject>();
            _poolDictionary[prefab] = objectPool;
        }

        objectPool.Enqueue(obj);
    }
}