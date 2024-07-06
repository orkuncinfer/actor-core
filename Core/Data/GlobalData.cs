using System;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalData
{
    private static Dictionary<string, Data> _datasets = new Dictionary<string, Data>();
    
    public static void LoadData<T>(string key,T data) where T : Data
    {
        string dataKey = data.GetType().ToString();
        Type dataType = data.GetType();
        if (key != "")
        {
            dataKey += ":" + key;
        }
        //Debug.Log("GlobalData: Loaded data with key : " + dataKey);
        
        if (_datasets.ContainsKey(dataKey))
        {
            _datasets[dataKey] = data;  
        }
        else
        {
            _datasets.Add(dataKey, data);  
        }
        GlobalDataDisplayer.Instance.FetchData(_datasets);
        Debug.Log($"$GlobalData: Loaded data with key : {dataKey}");
    }
    
    public static T GetData<T>(string key ="") where T : Data
    {
        Type dataType = typeof(T);
        
        string dataKey = typeof(T).ToString();
        if (key != "")
        {
            dataKey += ":" + key;
        }
        
        //Debug.Log("GlobalData: Tried to get data with key : " + dataKey);

        if (_datasets.ContainsKey(dataKey))
        {
            return (T)_datasets[dataKey];
        }

        Debug.LogError($"Data of type '{dataType}' not found! searched with key '{key}'");
        return null;
    }
}