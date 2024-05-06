using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

public class ItemAttribute
{
    [SerializeField]
    private string _attributeID;
    [FirestoreProperty]
    public string AttributeID
    {
        get => _attributeID;
        set => _attributeID = value;
    }
}
