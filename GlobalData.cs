using System;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalData
{
    private static Dictionary<Type, Data> _datasets = new Dictionary<Type, Data>();
    
    public static void LoadData<T>(T data) where T : Data
    {
        Type dataType = typeof(T);
        
        if (_datasets.ContainsKey(dataType))
        {
            _datasets[dataType] = data;  // Override existing data.
        }
        else
        {
            _datasets.Add(dataType, data);  // Add new data.
        }
    }

    public static T GetData<T>() where T : Data
    {
        Type dataType = typeof(T);

        if (_datasets.ContainsKey(dataType))
        {
            return (T)_datasets[dataType];
        }

        Debug.LogWarning($"Data of type '{dataType}' not found!");
        return null;
    }
}