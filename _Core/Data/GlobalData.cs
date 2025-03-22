using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public static class GlobalData
{
    private static readonly Dictionary<string, Data> _datasets = new Dictionary<string, Data>();

    public static event Action<Data> OnDataInstalled;

    public static void InstallData<T>(string key, T data) where T : Data
    {
        string dataKey = GetDataKey(data.GetType(), key);

        _datasets[dataKey] = data; // Direct assignment avoids redundant ContainsKey check

        data.OnInstalled();
        OnDataInstalled?.Invoke(data);
        
        GlobalDataDisplayer.Instance.FetchData(_datasets);

        Debug.Log($"GlobalData: Loaded data with key: {dataKey}");
    }

    public static T GetData<T>(string key = "") where T : Data
    {
        string dataKey = GetDataKey(typeof(T), key);
        
        if (_datasets.TryGetValue(dataKey, out Data foundData))
        {
            foundData.OnFirstTimeGet();
            return (T)foundData;
        }

        Debug.LogError($"Data of type '{typeof(T)}' not found! searched with key '{dataKey}'");
        return null;
    }
    
    public static bool TryGetData<T>(string key, out T data) where T : Data
    {
        string dataKey = GetDataKey(typeof(T), key);

        if (_datasets.TryGetValue(dataKey, out Data foundData))
        {
            data = (T)foundData;
            return true;
        }

        data = null;
        return false;
    }

    public static void SubscribeToDataInstalled(Action<Data> callback, string key, Type dataType)
    {
        if (callback == null) return;
        
        OnDataInstalled += callback;

        string dataKey = GetDataKey(dataType, key);
        
        if (_datasets.TryGetValue(dataKey, out Data existingData))
        {
            callback?.Invoke(existingData);
        }
    }

    private static readonly StringBuilder _sb = new StringBuilder();

    private static string GetDataKey(Type dataType, string key)
    {
        _sb.Clear();
        _sb.Append(dataType.ToString());

        if (!string.IsNullOrEmpty(key))
        {
            _sb.Append(':').Append(key);
        }

        return _sb.ToString();
    }
}
