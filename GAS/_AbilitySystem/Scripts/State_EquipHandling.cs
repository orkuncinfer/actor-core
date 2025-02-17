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
    
    private InventoryDefinition _equipmentInventory;
    private int _lastTriedSlotIndex;
        
    protected override void OnEnter()
    {
        base.OnEnter();

        _equipmentInventory = DefaultPlayerInventory.Instance.GetInventoryDefinition(EquipmentInventoryKey.ID);
        
        foreach (var abilityInfo in EquipActionNames)
        {
            var abilityAction = ActionAsset.FindAction(abilityInfo);
            abilityAction.performed += OnPerformed;
            abilityAction?.Enable();
        }
        
        EquipInventorySlot(0);
    }

    public void EquipInventorySlot(int slotIndex)
    {
        if (!_equipmentInventory.InventoryData.InventorySlots[slotIndex].ItemID.IsNullOrWhitespace())
        {
            DS_EquipmentUser equipmentUser = Owner.GetData<DS_EquipmentUser>();
            ItemDefinition itemDefinition = InventoryUtils.FindItemWithId(_equipmentInventory.InventoryData.InventorySlots[slotIndex].ItemID);
            Transform equipmentInSlot = Owner.SocketRegistry.SlotDictionary[itemDefinition.GetData<Data_Equippable>().UnequipSlotName];
            
            if (equipmentUser.EquipmentInstance != null)
            {
                if(equipmentUser.EquipmentInstance.GetComponent<Equippable>().ItemDefinition == itemDefinition)// we already equipped same item so unarm completely
                {
                    if(equipmentUser.EquipmentInstance.TryGetComponent(out Equippable equipable))
                    {
                        equipable.TryUnequipWithAbility();
                    }
                    return;
                }
                else // we are trying to equip different item so unequip current
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
                    equipable.TryEquipWithAbility();
                }
            }
            else
            {
                equipmentUser.EquipmentPrefab = itemDefinition.WorldPrefab;
            }
            
            
        }
    }

    private void OnCurrentUnequipped(ActiveAbility obj)
    {
        obj.onFinished -= OnCurrentUnequipped;
        EquipInventorySlot(_lastTriedSlotIndex);
    }

    protected override void OnExit()
    {
        base.OnExit();
        foreach (var abilityInfo in EquipActionNames)
        {
            var abilityAction = ActionAsset.FindAction(abilityInfo);
            abilityAction.performed -= OnPerformed;
        }
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