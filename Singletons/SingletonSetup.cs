using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[SOCreatable]
public class SingletonSetup : ScriptableObject
{
    [SerializeField] private  List<GameObject> _singletons = new List<GameObject>();
    
    public void Initialize()
    {
        foreach (GameObject prefab in _singletons)
        {
            Instantiate(prefab);
        }
    }

}
