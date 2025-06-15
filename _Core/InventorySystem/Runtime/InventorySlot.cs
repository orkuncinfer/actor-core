using System;
using System.Collections.Generic;
using System.ComponentModel;
using Firebase.Firestore;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

[FirestoreData]
[Serializable]
public class InventorySlot
{
    [ES3NonSerializable] [SerializeField] private GameplayTagContainer _allowedTags = new GameplayTagContainer();

    public GameplayTagContainer AllowedTags
    {
        get => _allowedTags;
        set => _allowedTags = value;
    }
    
    public string ItemID
    {
        get => _itemData.ItemID;
        set => _itemData.ItemID = value;
    }

    [SerializeField] private int _itemCount;

    public int ItemCount => ItemData.Quantity;


    [ShowInInspector]private ItemData _itemData;

    [FirestoreProperty]
    public ItemData ItemData
    {
        get => _itemData;
        set
        {
            if (_itemData != value)
            {
                ItemData oldValue = _itemData;
                _itemData = value;
                onItemDataChanged?.Invoke(oldValue,value,SlotIndex);
            }
        }
    }

    public int SlotIndex;

    public event Action<ItemData, ItemData, int> onItemDataChanged; 

    public void ResetSlot()
    {
        _itemData = null;
        _itemCount = 0;
    }
}