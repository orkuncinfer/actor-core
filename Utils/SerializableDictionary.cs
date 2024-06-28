using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<TKey, TValue>
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();

    [SerializeField]
    private List<TValue> values = new List<TValue>();

    public Dictionary<TKey, TValue> ToDictionary()
    {
        return keys.Zip(values, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
    }

    public void FromDictionary(Dictionary<TKey, TValue> dictionary)
    {
        keys.Clear();
        values.Clear();
        foreach (var kvp in dictionary)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public List<TKey> GetKeys()
    {
        if (keys.Count > 0)
        {
            return keys;
        }
        throw new KeyNotFoundException("The given key was not present in the dictionary.");
    }
    
    public void Add(TKey key, TValue value)
    {
        // Check if the key already exists
        if (keys.Contains(key))
        {
            return;
        }

        keys.Add(key);
        values.Add(value);
    }

    public bool Remove(TKey key)
    {
        int index = keys.IndexOf(key);
        if (index >= 0)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
            return true;
        }
        return false;
    }
    
    public TValue Get(TKey key)
    {
        int index = keys.IndexOf(key);
        if (index >= 0)
        {
            return values[index];
        }
        return default(TValue);
    }
    public void Set(TKey key,TValue value)
    {
        int index = keys.IndexOf(key);
        if (index >= 0)
        {
             values[index] = value;
        }
        
    }

    public bool Contains(TKey key)
    {
        int index = keys.IndexOf(key);
        if (index >= 0)
        {
            return true;
        }
        return false;
    }
    
    public void Clear()
    {
        keys.Clear();
        values.Clear();
    }
}