using System.Collections;
using System.Collections.Generic;
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
}

[System.Serializable]
public class SocketData
{
    public string Key;
    public Transform Transform;
}
