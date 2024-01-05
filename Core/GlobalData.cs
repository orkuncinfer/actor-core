using System;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalData
{
    private static Dictionary<string, Data> _datasets = new Dictionary<string, Data>();
    
    public static void LoadData<T>(string key,T data) where T : Data
    {
        string dataKey = key;
        
        Debug.Log("loaded data with key :" + dataKey);
        
        if (_datasets.ContainsKey(dataKey))
        {
            _datasets[dataKey] = data;  // Override existing data.
        }
        else
        {
            _datasets.Add(dataKey, data);  // Add new data.
        }
    }
    
    public static T GetData<T>(string key ="") where T : Data
    {
        Type dataType = typeof(T);
        
        string dataKey = key + dataType;
        
        Debug.Log("tried to get data with key :" + dataKey);

        if (_datasets.ContainsKey(dataKey))
        {
            return (T)_datasets[dataKey];
        }

        Debug.LogWarning($"Data of type '{dataType}' not found!");
        return null;
    }
}