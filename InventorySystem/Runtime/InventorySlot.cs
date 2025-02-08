using System;
using System.ComponentModel;
using Firebase.Firestore;
using JetBrains.Annotations;
using UnityEngine;

[FirestoreData][Serializable]
public class InventorySlot
{
    /*[SerializeField]
    private ItemData _itemData;
    [FirestoreProperty]
    public ItemData ItemData
    {
        get => _itemData;
        set => _itemData = value;
    }*/
    [ES3NonSerializable]
    [SerializeField] private GameplayTagContainer _allowedTags;
    public GameplayTagContainer AllowedTags
    {
        get => _allowedTags;
        set => _allowedTags = value;
    }
    
    [SerializeField]
    private string _itemID;
    [FirestoreProperty]
    public string ItemID
    {
        get => _itemID;
        set => _itemID = value;
    }
    [SerializeField]
    private int _itemCount;
    [FirestoreProperty]
    public int ItemCount
    {
        get => _itemCount;
        set => _itemCount = value;
    }

    
    public InventorySlot()
    {
        //ItemData = new ItemData();
    }
}