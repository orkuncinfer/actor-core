using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ES3Wrapper
{
    private static readonly Dictionary<string, int> _savedKeys = new Dictionary<string, int>();
    private static  readonly Dictionary<string, int> _loadedKeys = new Dictionary<string, int>();
    
    public static void Save(string key, object data, Transform transformInfo = null)
    {
        
        if (_savedKeys.ContainsKey(key))
        {
            Debug.LogError($"Tried to save data type of {data.GetType()} with key that is already in use!!! TransformInfo : {transformInfo}");
        }
        else
        {
            _savedKeys.Add(key, 1);
        }
        ES3.Save(key, data);
    }
    
    public static void LoadInto(string key, object data, Transform transformInfo = null)
    {
        if (_loadedKeys.ContainsKey(key))
        {
            Debug.LogError($"Tried to load data type of {data.GetType()} with key that is already in use!!! TransformInfo : {transformInfo}");
        }
        else
        {
            _loadedKeys.Add(key, 1);
        }
        ES3.LoadInto(key, data);
    }
}
