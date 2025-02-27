using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class State_EquipHandling : MonoState
{
    public InputActionAsset ActionAsset;
    public GenericKey EquipmentInventoryKey;

    [SerializeField] private string[] EquipActionNames;
    

    [SerializeField] private EventField<Equippable> _equipFromGround;

    [SerializeField] private AbilityDefinition _dropEquipment;

    private DS_EquipmentUser _equipmentUser;
    
    private int _lastTriedSlotIndex;
    private int _currentEquipIndex;
    private ItemData _currentEquippedItemData;
    private InventoryDefinition _equipmentInventory;
    
    protected override void OnEnter()
    {
        base.OnEnter();
        _equipmentUser = Owner.GetData<DS_EquipmentUser>();
        _equipFromGround.Register(Owner,OnEquipFromGround);
        _equipmentInventory = DefaultPlayerInventory.Instance.GetInventoryDefinition(EquipmentInventoryKey.ID);

        foreach (var slot in _equipmentInventory.InventoryData.InventorySlots)
        {
            slot.onItemDataChanged += OnSlotItemDataChanged;
        }
        
        foreach (var abilityInfo in EquipActionNames)
        {
            var abilityAction = ActionAsset.FindAction(abilityInfo);
            abilityAction.performed += OnPerformed;
            abilityAction?.Enable();
        }
        EquipInventorySlot(0);
    }

    private void OnSlotItemDataChanged(ItemData oldItem, ItemData newItem)
    {
        //reverse for loop
        bool foundInBag = false;
        for (int i = _equipmentUser._equippedInstances.Count - 1; i >= 0; i--)
        {
            if (_equipmentUser._equippedInstances[i].ItemData == oldItem)
            {
                _equipmentUser.DropEquippable(_equipmentUser._equippedInstances[i]);
                foundInBag = true;
            }
        }
        if (!foundInBag)
        {
            ItemDefinition oldItemDefinition = InventoryUtils.FindItemWithId(newItem.ItemID);
            GameObject oldInstance = PoolManager.SpawnObject(oldItemDefinition.WorldPrefab);
            oldInstance.GetComponent<Equippable>().ItemData = oldItem;
            oldInstance.GetComponent<Equippable>().DropInstance(Owner);
        }
        
        ItemDefinition itemDefinition = InventoryUtils.FindItemWithId(newItem.ItemID);
        GameObject newInstance = PoolManager.SpawnObject(itemDefinition.WorldPrefab);
        newInstance.GetComponent<Equippable>().ItemData = newItem;
        _equipmentUser.EquipWorldInstance(newInstance);
    }

    protected override void OnExit()
    {
        base.OnExit();
        foreach (var abilityInfo in EquipActionNames)
        {
            var abilityAction = ActionAsset.FindAction(abilityInfo);
            abilityAction.performed -= OnPerformed;
        }
        foreach (var slot in _equipmentInventory.InventoryData.InventorySlots)
        {
            slot.onItemDataChanged -= OnSlotItemDataChanged;
        }
        _equipFromGround.Unregister(Owner,OnEquipFromGround);
    }
    
    private void OnEquipFromGround(EventArgs arg1, Equippable arg2)
    {
        if (_equipmentInventory.ContainsItemAny(arg2.ItemDefinition, out int slotIndex))
        {
            if (slotIndex == _currentEquipIndex) // drop current then equip arg2
            {
                Vector3 spawnPosition = Owner.GetComponent<CapsuleCollider>().center + Owner.transform.position;
                GameObject instance = PoolManager.SpawnObject(arg2.ItemDefinition.WorldPrefab,spawnPosition,Quaternion.identity);
                instance.GetComponent<Equippable>().DropInstance(Owner);
            }
        }
    }

    public void EquipInventorySlot(int slotIndex)
    {
        if (!_equipmentInventory.InventoryData.InventorySlots[slotIndex].ItemID.IsNullOrWhitespace())
        {
            DS_EquipmentUser equipmentUser = _equipmentUser;
            ItemDefinition itemDefinition = InventoryUtils.FindItemWithId(_equipmentInventory.InventoryData.InventorySlots[slotIndex].ItemID);
            
            Transform equipmentInSlot = Owner.SocketRegistry.SlotDictionary[itemDefinition.GetData<Data_Equippable>().UnequipSlotName];
            
            if (equipmentUser.EquipmentInstance != null) //already have equipped on hand
            {
                if(equipmentUser.EquipmentInstance.GetComponent<Equippable>().ItemDefinition == itemDefinition)// we already equipped same item so unarm completely
                {
                    if(equipmentUser.EquipmentInstance.TryGetComponent(out Equippable equipable))
                    {
                        if(equipable.TryUnequipWithAbility() != null);
                            _currentEquipIndex = -1;
                    }
                    return;
                }
                else // we are trying to equip different item so unequip current then equip next when finished
                {
                    if(equipmentUser.EquipmentInstance.TryGetComponent(out Equippable equipable))
                    {
                       ActiveAbility unequipAbility = equipable.TryUnequipWithAbility();
                       if(unequipAbility != null)
                       {
                           _lastTriedSlotIndex = slotIndex;
                           unequipAbility.onFinished += OnCurrentUnequipped;
                           return;
                       }
                       else
                       {
                           return;
                       }
                    }
                }
            }
            
            if (equipmentInSlot != null)
            {
                if(equipmentInSlot.TryGetComponent(out Equippable equipable))
                {
                    equipmentUser.ItemToEquip = equipable.gameObject;
                    if (equipable.TryEquipWithAbility() != null)
                    {
                        _currentEquipIndex = slotIndex;
                        _currentEquippedItemData = _equipmentInventory.InventoryData.InventorySlots[slotIndex].ItemData;
                    }
                }
            }
            else
            {
                GameObject newInstance = PoolManager.SpawnObject(itemDefinition.WorldPrefab);
                newInstance.GetComponent<Equippable>().ItemData = _equipmentInventory.InventoryData.InventorySlots[slotIndex].ItemData;
                _equipmentUser.EquipWorldInstance(newInstance);
                //equipmentUser.EquipmentPrefab = itemDefinition.WorldPrefab;
                _currentEquipIndex = slotIndex;
                _currentEquippedItemData = _equipmentInventory.InventoryData.InventorySlots[slotIndex].ItemData;
            }
        }
    }

    private void OnCurrentUnequipped(ActiveAbility obj)
    {
        obj.onFinished -= OnCurrentUnequipped;
        EquipInventorySlot(_lastTriedSlotIndex);
    }
    
    private void OnPerformed(InputAction.CallbackContext obj)
    {
        var abilityTriggerInfo = obj.action.name;
        for (int i = 0; i < EquipActionNames.Length; i++)
        {
            if (EquipActionNames[i] == abilityTriggerInfo)
            {
                EquipInventorySlot(i);
            }
        }

        /* _gasData.AbilityController.TryActiveAbilityWithDefinition(abilityTriggerInfo.AbilityDefinition, out ActiveAbility activatedAbility);

        if (activatedAbility != null)
        {
            IsBusy = true;
            activatedAbility.onFinished += OnAbilityFinished;
        }*/
    }

    private void OnAbilityFinished(ActiveAbility obj)
    {
        obj.onFinished -= OnAbilityFinished;
        EquipInventorySlot(_lastTriedSlotIndex);
    }
}