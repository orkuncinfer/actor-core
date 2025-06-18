using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ItemDropInstance : Collectible
{
    public ItemDefinition ItemDefinition;
    public InventoryDefinition InventoryDefinition;
    public GenericKey InventoryKey;
    public int DropCount;
    public WorldItemLabel LabelInstance;
    public Transform ModelHolder;

    [SerializeField] private bool _isEquippable; 
    [ShowIf("_isEquippable")]public GenericKey EquipmentInventoryKey;

    public ItemData _itemData;
    private GameObject _model;

    public void OnMovementEnd()
    {
        LabelInstance.ActivateLabel();
    }

    private void OnEnable()
    {
        SpawnModel();
    }

    private void SpawnModel()
    {
        ItemDefinition = InventoryUtils.FindItemDefinitionWithId(_itemData.ItemID);
        if(ItemDefinition==null) return;
        if(ItemDefinition.Model == null) return;
        if(_model != null) PoolManager.ReleaseObject(_model);
        _model = PoolManager.SpawnObject(ItemDefinition.Model);
        _model.transform.position = ModelHolder.transform.position;
        _model.transform.rotation = ModelHolder.transform.rotation;
        _model.transform.SetParent(ModelHolder,true);
    }
    
    public void SetItemData(ItemData itemData)
    {
        _itemData = itemData;
        
        SpawnModel();
    }
    
    [Button]
    public override void Collect()
    {
        InventoryDefinition collectInventory = null;
        
        if(InventoryDefinition) collectInventory = InventoryDefinition;
       // if (InventoryKey) collectInventory = DefaultPlayerInventory.Instance.GetInventoryDefinition(InventoryKey.ID);
        ItemDefinition = InventoryUtils.FindItemDefinitionWithId(_itemData.ItemID);
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
            //collectInventory = DefaultPlayerInventory.Instance.GetInventoryDefinition(EquipmentInventoryKey.ID);
            if (collectInventory.ReplaceItem(ItemDefinition, _itemData))
            {
                if(LabelInstance)ItemDropManager.Instance.PickedUp(LabelInstance);
                PoolManager.ReleaseObject(this.gameObject);
                return;
            }
            Debug.LogError("Couldn't equip");
        }
    }
}
