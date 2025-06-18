using FishNet.Object;
using UnityEngine;

public class MobManager : NetworkBehaviour
{
    public static MobManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject RequestSpawn(GameObject prefab, Vector3 position)
    {
        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        
        base.Spawn(go);
        return go;
    }

    public bool CanSpawn()
    {
        return base.IsServer;
    }
}