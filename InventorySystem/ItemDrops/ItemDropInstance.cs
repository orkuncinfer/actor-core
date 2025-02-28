using System;
using System.Collections;
using System.Collections.Generic;
using NetworkShared.Packets.ServerClient;
using Sirenix.OdinInspector;
using UnityEngine;

public class ItemDropInstance : Collectible
{
    public ItemDefinition ItemDefinition;
    public InventoryDefinition InventoryDefinition;
    public GenericKey InventoryKey;
    public int DropCount;
    public WorldItemLabel LabelInstance;

    [SerializeField] private bool _isEquippable; 
    [ShowIf("_isEquippable")]public GenericKey EquipmentInventoryKey;
    
    [ShowInInspector]public FbGeneratedItemResult GeneratedItemResult;

    private ItemData _itemData;

    public void OnMovementEnd()
    {
        LabelInstance.ActivateLabel();
    }

    private void OnEnable()
    {
        /*if(ItemDefinition.WorldPrefab)
            Instantiate(ItemDefinition.WorldPrefab, transform.position, Quaternion.identity);*/
    }

    public void SetItemData(ItemData itemData)
    {
        _itemData = itemData;
    }
    
    [Button]
    public override void Collect()
    {
        InventoryDefinition collectInventory = null;
        
        if(InventoryDefinition) collectInventory = InventoryDefinition;
        if (InventoryKey) collectInventory = DefaultPlayerInventory.Instance.GetInventoryDefinition(InventoryKey.ID);
        ItemData newItemData = new ItemData
        {
            ItemID = ItemDefinition.ItemID,
            UniqueID = "dropped"
        };
        int added = collectInventory.AddItem(ItemDefinition, DropCount,newItemData);
        if (added > 0)
        {
            DropCount -= added;
            if (DropCount == 0)
            {
                if(LabelInstance)ItemDropManager.Instance.PickedUp(LabelInstance);
                PoolManager.ReleaseObject(this.gameObject);
            }
        }
    }

    public override bool IsEquippable()
    {
        return _isEquippable;
    }

    public override void Equip()
    {
        InventoryDefinition collectInventory = null;
        if (_isEquippable)
        {
            collectInventory = DefaultPlayerInventory.Instance.GetInventoryDefinition(EquipmentInventoryKey.ID);
            ItemData newItemData = new ItemData
            {
                ItemID = ItemDefinition.ItemID,
                UniqueID = "dropped"
            };
            if (collectInventory.ReplaceItem(ItemDefinition, DropCount, newItemData))
            {
                if(LabelInstance)ItemDropManager.Instance.PickedUp(LabelInstance);
                PoolManager.ReleaseObject(this.gameObject);
                return;
            }
            Debug.LogError("Couldn't equip");
        }
    }
}
