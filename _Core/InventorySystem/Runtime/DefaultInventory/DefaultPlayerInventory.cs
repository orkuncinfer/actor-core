using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public  class DefaultPlayerInventory : PersistentSingleton<DefaultPlayerInventory>
{
    [ShowInInspector] protected Dictionary<string, int> _inventory = new Dictionary<string, int>();

    [ValueDropdown("GetAllItems")]
    [SerializeField] private ItemBaseDefinition _testItem;
    
    [ShowInInspector]public Dictionary<string,InventoryDefinition> InventoryDefinitions = new Dictionary<string, InventoryDefinition>();
    
    public event Action<string,int,int> onItemAdded;
    public event Action<string,int,int> onItemRemoved;
    public event Action<string, int,int> onItemChanged; 
    
    [Button]
    private void AddItemTest(int amount = 1)
    {
        AddItem(_testItem.ItemId, amount);
    }

    private void OnDestroy()
    {
        ES3.Save("DefaultInventory",_inventory);
    }

    private void Start()
    {
        _inventory = ES3.Load("DefaultInventory",_inventory);
    }

    public void AddItem(string itemId, int amount)
    {
        GetInventory();
        ItemBaseDefinition item = InventoryUtils.FindItemDefinitionWithId(itemId);
        if (item != null)
        {
            if (item is ItemDefinition itemDef)
            {
                foreach (var action in itemDef.ItemActions)
                {
                    action.OnAction(ActorRegistry.PlayerActor);
                }
            }
        }
        int oldAmount = GetItemCount(itemId);
        if (_inventory.ContainsKey(itemId))
        {
            _inventory[itemId] += amount;
        }
        else
        {
            _inventory.Add(itemId, amount);
        }
        
        
        onItemAdded?.Invoke(itemId,oldAmount,GetItemCount(itemId));
        onItemChanged?.Invoke(itemId,oldAmount,GetItemCount(itemId));
    }
    
    public void RemoveItem(string itemId, int amount)
    {
        GetInventory();
        int oldAmount = GetItemCount(itemId);
        if(oldAmount == 0) return;
        if (_inventory.ContainsKey(itemId))
        {
            _inventory[itemId] -= amount;
            if (_inventory[itemId] <= 0)
            {
                _inventory.Remove(itemId);
            }
            
            onItemRemoved?.Invoke(itemId,oldAmount,GetItemCount(itemId));
            onItemChanged?.Invoke(itemId,oldAmount,GetItemCount(itemId));
        }
    }
    
    public int GetItemCount(string itemId)
    {
        GetInventory();
        if (_inventory.ContainsKey(itemId))
        {
            return _inventory[itemId];
        }
        return 0;
    }
  
    
    public void Clear()
    {
        GetInventory();
        _inventory.Clear();
    }
    
    public bool HasItem(string itemId)
    {
        GetInventory();
        return _inventory.ContainsKey(itemId);
    }
    
    public Dictionary<string, int> GetInventory()
    {
        _inventory = _inventory;
        return _inventory;
    }
    
    public ItemBaseDefinition GetItemDefinition(string itemKey)
    {
        return InventoryUtils.FindItemDefinitionWithId(itemKey);
    }
    
#if UNITY_EDITOR
    private List<ValueDropdownItem<ItemDefinition>> GetAllItems()
    {
        var allKeys = Resources.FindObjectsOfTypeAll<ItemDefinition>();
        var dropdownItems = new List<ValueDropdownItem<ItemDefinition>>();
        foreach (var key in allKeys)
        {
            dropdownItems.Add(new ValueDropdownItem<ItemDefinition>(key.name, key));
        }

        return dropdownItems;
    }
#endif
  
}
