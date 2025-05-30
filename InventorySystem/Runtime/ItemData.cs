using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Firebase.Firestore;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
[FirestoreData]
public class ItemData 
{
    [FirestoreProperty][ShowInInspector]
    public string ItemID { get; set; }
    
    [FirestoreProperty][ShowInInspector]
    public string UniqueID { get; set; }
    
    [ShowInInspector]
    public int Quantity { get; set; }
    
    [ShowInInspector] [ES3Serializable]
    private Dictionary<string, string> _attributes = new Dictionary<string, string>();

    [FirestoreProperty]
    public Dictionary<string, string> Attributes
    {
        get => _attributes;
        set => _attributes = value;
    }

    public event Action<string,string> onAttributeChanged;

    public bool TryGetAttribute(string key, out string value)
    {
        value = "";

        if (_attributes == null) return false;
        if (_attributes.TryGetValue(key, out var attribute))
        {
            value = attribute;
            return true;
        }

        return false;
    }
    public void SetAttribute(string key, string value)
    {
        if (_attributes == null) _attributes = new Dictionary<string, string>();
        _attributes[key] = value;
        onAttributeChanged?.Invoke(key,value);
    }
}