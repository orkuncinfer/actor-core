using UnityEngine;

public class State_BindEquipmentToInventory : MonoState
{
    [SerializeField] private InventoryDefinition _inventoryDefinition;

    private DS_EquipmentUser _equipmentUser;

    private bool _registered;

    protected override void OnEnter()
    {
        base.OnEnter();
        _equipmentUser = Owner.GetData<DS_EquipmentUser>();
        
        if (_inventoryDefinition.IsInitialized)
        {
            Init();    
        }
        else
        {
            _registered = true;
            _inventoryDefinition.onInventoryInitialized += Init;
        }
    }

    protected override void OnExit()
    {
        base.OnExit();
        if (_registered)
        {
            _inventoryDefinition.onInventoryInitialized -= Init;
        }
        
        foreach (var slot in _inventoryDefinition.InventoryData.InventorySlots)
        {
            slot.onItemDataChanged -= OnItemDataChanged;
        }
    }
    
    private void Init()
    {
        foreach (var slot in _inventoryDefinition.InventoryData.InventorySlots)
        {
            slot.onItemDataChanged += OnItemDataChanged;
            if (slot.ItemData == null)
            {
                _equipmentUser.EquipToSlot(slot.SlotIndex,null);
                continue;
            }
            else
            {
                ItemDefinition item = InventoryUtils.FindItemDefinitionWithId(slot.ItemData.ItemID);
                Debug.Log("Found item is " + item);
                 GameObject equipPrefab = InventoryUtils.FindItemDefinitionWithId(slot.ItemData.ItemID).WorldPrefab;
                _equipmentUser.EquipToSlot(slot.SlotIndex,equipPrefab);
            }
        }
    }

    private void OnItemDataChanged(ItemData arg1, ItemData arg2, int arg3)
    {
        if (arg2 == null)
        {
            _equipmentUser.EquipToSlot(arg3,null);
        }
        else
        {
            ItemDefinition item = InventoryUtils.FindItemDefinitionWithId(arg2.ItemID);
            Debug.Log("Found item is " + item);
            GameObject equipPrefab = InventoryUtils.FindItemDefinitionWithId(arg2.ItemID).WorldPrefab;
            _equipmentUser.EquipToSlot(arg3,equipPrefab);
        }
    }
}
