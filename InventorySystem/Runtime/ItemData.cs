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
    [SerializeField] private string _itemID;

    [FirestoreProperty]
    public string ItemID
    {
        get => _itemID;
        set => _itemID = value;
    }
    
    [SerializeField]
    private string _uniqueID;
    [FirestoreProperty]
    public string UniqueID
    {
        get => _uniqueID;
        set => _uniqueID = value;
    }
    
    [ShowInInspector] [ES3Serializable]
    private Dictionary<string, string> _attributes = new Dictionary<string, string>();

    [FirestoreProperty]
    public Dictionary<string, string> Attributes
    {
        get => _attributes;
        set => _attributes = value;
    }

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
}