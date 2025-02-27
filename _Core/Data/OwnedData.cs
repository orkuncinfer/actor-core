using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public static class OwnedData
{
    private readonly struct DataOwner
    {
        public readonly ActorBase Owner;
        public readonly string Key;

        public DataOwner(ActorBase owner, string key)
        {
            Owner = owner;
            Key = key;
        }

        public override bool Equals(object obj)
        {
            if (obj is DataOwner other)
            {
                return ReferenceEquals(Owner, other.Owner) && Key == other.Key;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (Owner?.GetHashCode() ?? 0) ^ Key.GetHashCode();
        }
    }

    private static readonly Dictionary<DataOwner, Data> _datasets = new Dictionary<DataOwner, Data>();
    public static event Action<Data> OnDataInstalled; 

    public static void LoadData<T>(ActorBase actor, string key, T data) where T : Data
    {
        var owner = new DataOwner(actor, key);

        _datasets[owner] = data; // Directly assigning avoids redundant ContainsKey check

        data.OnInstalled();
        OnDataInstalled?.Invoke(data);

        Debug.Log($"GlobalData: Loaded data with key : {GetDataKey(data.GetType(), key)}");
    }

    public static T GetData<T>(this ActorBase actor, string key = "") where T : Data
    {
        var owner = new DataOwner(actor, GetDataKey(typeof(T), key));
        if (_datasets.TryGetValue(owner, out Data data))
        {
            data.OnFirstTimeGet();
            return (T)data;
        }

        Debug.LogError($"Data of type '{typeof(T)}' not found! searched with key '{key}'");
        return null;
    }

    public static bool TryGetData<T>(this ActorBase actor, string key, out T data) where T : Data
    {
        var owner = new DataOwner(actor, GetDataKey(typeof(T), key));
        if (_datasets.TryGetValue(owner, out Data foundData))
        {
            data = (T)foundData;
            return true;
        }

        data = null;
        return false;
    }

    public static void SubscribeToDataInstalled(this ActorBase actor, Action<Data> callback, string key, Type dataType)
    {
        if (callback == null) return;
        
        OnDataInstalled += callback;

        var owner = new DataOwner(actor, GetDataKey(dataType, key));

        if (_datasets.TryGetValue(owner, out Data existingData))
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
