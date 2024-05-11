using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class SocketRegistry : MonoBehaviour
{
    public List<SocketData> Sockets;

    public Dictionary<string, Transform> SocketDictionary = new Dictionary<string, Transform>();
    private void Awake()
    {
        for (int i = 0; i < Sockets.Count; i++)
        {
            SocketDictionary.Add(Sockets[i].Key,Sockets[i].Transform);
        }
    }
    public Transform GetSocket(string key)
    {
        if (SocketDictionary.TryGetValue(key, out Transform socket))
        {
            return socket;
        }
        else
        {
            Debug.LogError($"{key} , could not found in the socket registry in : {gameObject.name}");
        }
        return null;
    }

    [Button]
    public void FindTransformsInRig()
    {
        foreach (SocketData socket in Sockets)
        {
            Debug.Log("searching for " + socket.Key);
            if (socket.Transform == null)
            {
                if(GetChildGameObject(gameObject,socket.Key) == null) continue;
                socket.Transform = GetChildGameObject(gameObject, socket.Key).transform;
            }
        }
    }
    public GameObject GetChildGameObject(GameObject fromGameObject, string withName)
    {
        var allKids = fromGameObject.GetComponentsInChildren<Transform>();
        var kid = allKids.FirstOrDefault(k => k.gameObject.name == withName);
        if (kid == null) return null;
        return kid.gameObject;
    }
}

[System.Serializable]
public class SocketData
{
    public string Key;
    public Transform Transform;
}
