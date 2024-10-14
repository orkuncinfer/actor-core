using System;
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
        //ReInstantiateSockets();
        
        for (int i = 0; i < Sockets.Count; i++)
        {
            SocketDictionary.Add(Sockets[i].Key,Sockets[i].Transform);
        }
    }

    public void ReInstantiateSockets()
    {
        // Store the original list of transforms to avoid modifying it while iterating
        List<Transform> originalTransforms = Sockets.Select(s => s.Transform).ToList();

        for (int i = 0; i < Sockets.Count; i++)
        {
            if(Sockets[i].IsBone) continue;
            Transform originalTransform = originalTransforms[i];

            // Create an empty GameObject to replace the original
            GameObject instance = new GameObject(originalTransform.name);

            // Set position, rotation, and parent of the new GameObject
            instance.transform.position = originalTransform.position;
            instance.transform.rotation = originalTransform.rotation;
            instance.transform.SetParent(originalTransform.parent, false);

            // Copy over components if needed (optional, can be expanded)
            // Example: If the original object had custom components, they can be copied over.
            // You can add custom logic here to handle specific component copying if needed.

            // Assign the new GameObject's transform to the SocketData
            Sockets[i].Transform = instance.transform;

            // Destroy the original GameObject
            Destroy(originalTransform.gameObject);
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
    [HorizontalGroup()]public Transform Transform;
    [HorizontalGroup(100)] public bool IsBone;
}
