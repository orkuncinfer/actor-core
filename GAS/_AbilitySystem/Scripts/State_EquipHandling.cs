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
        _equipmentInventory = DefaultPlayerInventory.Instance.GetInventoryDefinition(EquipmentInventoryKey.ID);

        _equipmentUser.onEquipmentDropped += OnEquipmentDropped;
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
    protected override void OnExit()
    {
        base.OnExit();
        _equipmentUser.onEquipmentDropped -= OnEquipmentDropped;
        foreach (var abilityInfo in EquipActionNames)
        {
            var abilityAction = ActionAsset.FindAction(abilityInfo);
            abilityAction.performed -= OnPerformed;
        }
        foreach (var slot in _equipmentInventory.InventoryData.InventorySlots)
        {
            slot.onItemDataChanged -= OnSlotItemDataChanged;
        }
    }
    private void OnEquipmentDropped(Equippable obj)
    {
        foreach (var inventorySlot in _equipmentInventory.InventoryData.InventorySlots)
        {
            if (inventorySlot.ItemData == obj.ItemData)
            {
               DropPickableInstance(obj.ItemData);
                
                inventorySlot.ResetSlot();
            }
        }
    }

    private void DropPickableInstance(ItemData itemData)
    {
        if(itemData == null )return;
        Vector3 spawnPos = Owner.transform.position + new Vector3(0, 1, 0);
      
        ItemDefinition itemDefinition = InventoryUtils.FindItemWithId(itemData.ItemID);
        GameObject spawned = PoolManager.SpawnObject(itemDefinition.DropPrefab,spawnPos,Quaternion.identity);
        spawned.GetComponent<ItemDropInstance>().SetItemData(itemData);
        
        Collider charCollider = Owner.GetComponent<Collider>();
        Collider pickableCollider = spawned.GetComponent<Collider>();
        Physics.IgnoreCollision(charCollider,pickableCollider);

        Rigidbody rigid = spawned.GetComponent<Rigidbody>();
        rigid.isKinematic = false;
        rigid.AddForce(Owner.transform.forward * 5,ForceMode.Impulse);
    }

    private void OnSlotItemDataChanged(ItemData oldItem, ItemData newItem)
    {
        if (oldItem != null)
        {
            if (!string.IsNullOrEmpty(oldItem.ItemID))
            {
                bool foundInBag = false;
                for (int i = _equipmentUser._equippedInstances.Count - 1; i >= 0; i--)
                {
                    if (_equipmentUser._equippedInstances[i].ItemData == oldItem)
                    {
                        DropPickableInstance(oldItem);
                        
                        GameObject equipInstance = _equipmentUser._equippedInstances[i].gameObject;
                        _equipmentUser.DropEquippable(_equipmentUser._equippedInstances[i]);
                        PoolManager.ReleaseObject(equipInstance);
                        foundInBag = true;
                    }
                }
                if (!foundInBag)
                {
                    DropPickableInstance(oldItem);
                }
            }
        }
        
        ItemDefinition itemDefinition = InventoryUtils.FindItemWithId(newItem.ItemID);
        GameObject newInstance = PoolManager.SpawnObject(itemDefinition.WorldPrefab);
        newInstance.GetComponent<Equippable>().ItemData = newItem;
        _equipmentUser.EquipWorldInstance(newInstance);
    }

    public void EquipInventorySlot(int slotIndex)
    {
        if (_equipmentInventory.InventoryData.InventorySlots[slotIndex].ItemData != null)
        {
            DS_EquipmentUser equipmentUser = _equipmentUser;
            string itemID = _equipmentInventory.InventoryData.InventorySlots[slotIndex].ItemID;
            if(string.IsNullOrEmpty(itemID))return;
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