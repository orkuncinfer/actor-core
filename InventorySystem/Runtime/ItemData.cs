using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Firebase.Firestore;
using UnityEngine;

[Serializable]
[FirestoreData]
public class ItemData 
{
    [SerializeField]
    private string _itemID;
    [FirestoreProperty]
    public string ItemID
    {
        get => _itemID;
        set => _itemID = value;
    }
    [SerializeField]
    private string _ownerID;
    [FirestoreProperty]
    public string OwnerID
    {
        get => _ownerID;
        set => _ownerID = value;
    }
    
    [SerializeField]
    private string _name;
    [FirestoreProperty]
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    [SerializeField]
    private int _upgradeLevel;
    [FirestoreProperty]
    public int UpgradeLevel
    {
        get => _upgradeLevel;
        set => _upgradeLevel = value;
    }

    [SerializeField]
    private ItemAttribute[] _attributes;
    [FirestoreProperty]
    public ItemAttribute[] Attributes
    {
        get => _attributes;
        set => _attributes = value;
    }

    [SerializeField]
    private ItemRarity _itemRarity = ItemRarity.Common;
    [FirestoreProperty]
    public ItemRarity ItemRarity
    {
        get => _itemRarity;
        set => _itemRarity = value;
    }
}