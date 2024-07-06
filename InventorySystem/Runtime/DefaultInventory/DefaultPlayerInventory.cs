using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public  class DefaultPlayerInventory : Singleton<DefaultPlayerInventory>
{
    [ShowInInspector] protected Dictionary<string, int> _inventory;
    
    public event Action<string,int,int> onItemAdded;
    public event Action<string,int,int> onItemRemoved;
    public event Action<string, int,int> onItemChanged; 
    
    [Button]
    public void AddItem(string itemId, int amount)
    {
        GetInventory();
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
    
    void GetInventory()
    {
        _inventory = GlobalData.GetData<DS_PlayerPersistent>().Inventory;
    }

    protected override void Awake()
    {
        base.Awake();
        //_inventory = GlobalData.GetData<DS_PlayerPersistent>().Inventory;
    }
}
