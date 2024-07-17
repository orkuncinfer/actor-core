using System;
using System.Collections.Generic;
using MonsterLove.Collections;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class PoolManager : PersistentMonoSingleton<PoolManager>
{
	public bool logStatus;
	public Transform root;

	[ShowInInspector]private Dictionary<GameObject, ObjectPool<GameObject>> prefabLookup;
	[ShowInInspector]private Dictionary<GameObject, ObjectPool<GameObject>> instanceLookup; 
	
	private bool dirty = false;
	
	void Awake () 
	{
		prefabLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
		instanceLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
	}

	void Update()
	{
		if(logStatus && dirty)
		{
			PrintStatus();
			dirty = false;
		}
	}

	public void warmPool(GameObject prefab, int size)
	{
		if(prefabLookup.ContainsKey(prefab))
		{
			throw new Exception("Pool for prefab " + prefab.name + " has already been created");
		}
		var pool = new ObjectPool<GameObject>(() => { return InstantiatePrefab(prefab); }, size);
		prefabLookup[prefab] = pool;

		dirty = true;
	}

	public GameObject spawnObject(GameObject prefab)
	{
		return spawnObject(prefab, Vector3.zero, Quaternion.identity);
	}

	public GameObject spawnObject(GameObject prefab, Vector3 position, Quaternion rotation,Transform parent = null)
	{
		if (!prefabLookup.ContainsKey(prefab))
		{
			WarmPool(prefab, 1);
		}

		var pool = prefabLookup[prefab];

		var clone = pool.GetItem();
		clone.transform.SetPositionAndRotation(position, rotation);
		if(parent != null) 
			clone.transform.SetParent(parent,false);
		clone.SetActive(true);

		instanceLookup.Add(clone, pool);
		dirty = true;
		return clone;
	}

	public void releaseObject(GameObject clone)
	{
		clone.SetActive(false);

		if(instanceLookup.ContainsKey(clone))
		{
			instanceLookup[clone].ReleaseItem(clone);
			instanceLookup.Remove(clone);
			dirty = true;
		}
		else
		{
			Debug.LogWarning("No pool contains the object: " + clone.name);
		}
	}


	private GameObject InstantiatePrefab(GameObject prefab, Transform parent = null)
	{
		var go = Instantiate(prefab) as GameObject;
		if (root != null) go.transform.parent = root;
		return go;
	}

	public void PrintStatus()
	{
		foreach (KeyValuePair<GameObject, ObjectPool<GameObject>> keyVal in prefabLookup)
		{
			Debug.Log(string.Format("Object Pool for Prefab: {0} In Use: {1} Total {2}", keyVal.Key.name, keyVal.Value.CountUsedItems, keyVal.Value.Count));
		}
	}

	#region Static API
	
	public static event Action onBeforeReturnToPool;

	public static void WarmPool(GameObject prefab, int size)
	{
		Instance.warmPool(prefab, size);
	}

	public static GameObject SpawnObject(GameObject prefab)
	{
		return Instance.spawnObject(prefab);
	}

	public static GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation,Transform parent = null)
	{
		return Instance.spawnObject(prefab, position, rotation, parent);
	}

	public static void ReleaseObject(GameObject clone)
	{
		onBeforeReturnToPool?.Invoke();
		Instance.releaseObject(clone);
	}

	#endregion
}


