using System;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalData
{
    private static Dictionary<string, Data> _datasets = new Dictionary<string, Data>();
    private static HashSet<string> _installedDataKeys = new HashSet<string>();

    public static event Action<Data> OnDataInstalled; 

    public static void LoadData<T>(string key, T data) where T : Data
    {
        string dataKey = GetDataKey(data.GetType(), key);

        if (_datasets.ContainsKey(dataKey))
        {
            _datasets[dataKey] = data;
        }
        else
        {
            _datasets.Add(dataKey, data);
        }
        
        data.OnInstalled();
        _installedDataKeys.Add(dataKey);
        OnDataInstalled?.Invoke(data);
        GlobalDataDisplayer.Instance.FetchData(_datasets);
        Debug.Log($"GlobalData: Loaded data with key : {dataKey}");
    }

    public static T GetData<T>(string key = "") where T : Data
    {
        string dataKey = GetDataKey(typeof(T), key);

        if (_datasets.ContainsKey(dataKey))
        {
            _datasets[dataKey].OnFirstTimeGet();
            return (T)_datasets[dataKey];
        }

        Debug.LogError($"Data of type '{typeof(T)}' not found! searched with key '{key}'");
        return null;
    }
    
    public static bool TryGetData<T>(string key, out T data) where T : Data
    {
        if (_datasets.ContainsKey(key + typeof(T).ToString()))
        {
            data = (T) _datasets[key + typeof(T).ToString()];
            return true;
        }

        data = null;
        return false;
    }

    public static void SubscribeToDataInstalled(Action<Data> callback, string key, Type dataType)
    {
        OnDataInstalled += callback;
        string dataKey = GetDataKey(dataType, key);

        if (_installedDataKeys.Contains(dataKey))
        {
            callback?.Invoke(_datasets[dataKey]);
        }
    }

    private static string GetDataKey(Type dataType, string key)
    {
        string dataKey = dataType.ToString();
        if (!string.IsNullOrEmpty(key))
        {
            dataKey += ":" + key;
        }
        return dataKey;
    }
}
