using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class ItemAttribute
{
    [SerializeField]
    private string _key;
    [FirestoreProperty]
    public string Key
    {
        get => _key;
        set => _key = value;
    }
    
    [SerializeField]
    private string _value;
    [FirestoreProperty]
    public string Value
    {
        get => _value;
        set => _value = value;
    }
}
