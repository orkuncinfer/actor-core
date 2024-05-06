using System;
using System.Collections.Generic;
using Firebase.Firestore;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable][FirestoreData]
public class InventoryData
{
    [SerializeField]
    private int _slotCount;
    [FirestoreProperty]
    public int SlotCount
    {
        get => _slotCount;
        set => _slotCount = value;
    }
    
    [SerializeField]
    private List<InventorySlot> _inventorySlots;
    [FirestoreProperty]
    public List<InventorySlot> InventorySlots
    {
        get => _inventorySlots;
        set => _inventorySlots = value;
    }
}