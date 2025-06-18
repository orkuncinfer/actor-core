using System;
using Sirenix.Utilities;
using UnityEngine;

public class UI_EquipmentHudControl : MonoBehaviour
{
    [SerializeField] private Sherbert.Framework.Generic.SerializableDictionary<int, UI_EquippableHud> _equipmentSlots;
    [SerializeField] private InventoryDefinition _equipmentInventory;
    private ActorBase _owner;

    private DS_EquipmentUser _equipmentUser;

    private void Start()
    {
        _owner = ActorUtilities.FindFirstActorInParents(transform);
        _equipmentUser = _owner.GetData<DS_EquipmentUser>();
        
    }

    private void Awake()
    {
        _equipmentInventory.onInventoryInitialized += OnInventoryInit;
    }

    private void OnInventoryInit()
    {
        _owner = ActorUtilities.FindFirstActorInParents(transform);
        _equipmentUser = _owner.GetData<DS_EquipmentUser>();
        
        _equipmentInventory.onInventoryInitialized -= OnInventoryInit;
        
        _equipmentUser.onEquippedListChanged += OnEquippedListChanged;
    }

    private void OnEquippedListChanged()
    {
        foreach (var slot in _equipmentSlots)
        {
            bool found = false;
            foreach (var equippable in _equipmentUser._equippedInstances)
            {
                if(found) continue;
                ItemData itemData = _equipmentInventory.InventoryData.InventorySlots[slot.Key].ItemData;
                if (equippable.ItemData == itemData)
                {
                    _equipmentSlots[slot.Key].SetEquippable(equippable);
                    found = true;
                }
                //Debug.Log($"checking if {equippable.ItemData.ItemID} equals {arg2.ItemID}");
                //_equipmentSlots[slot.Key].SetEquippable(null);
            }
            if(found == false) _equipmentSlots[slot.Key].SetEquippable(null);
        }
    }

    private void OnSlotItemDataChangedWrapper(ItemData arg1, ItemData arg2, int slot)
    {
        OnSlotItemDataChanged(arg1, arg2, slot);
    }
    private void OnSlotItemDataChanged(ItemData arg1, ItemData arg2, int slotIndex)
    {
        
        foreach (var equippable in _equipmentUser._equippedInstances)
        {
            Debug.Log($"checking if {equippable.ItemData.ItemID} equals {arg2.ItemID}");
            if (equippable.ItemData == arg2)
            {
                _equipmentSlots[slotIndex].SetEquippable(equippable);
                return;
            }
        }
        _equipmentSlots[slotIndex].SetEquippable(null);
    }
}