using System;
using UnityEngine;

[Serializable]
public class DS_StatUpgradeItem : Data
{
    [SerializeField] private string _statName;
    public string StatName => _statName;

    [SerializeField] private AnimationCurve _costCurve;

    [SerializeField] private ItemBaseDefinition _paymentItem;
    public ItemBaseDefinition PaymentItem => _paymentItem;

    [SerializeField] private int _maxUpgradeLevel = 99;


    public bool CanPurchase(ItemBaseDefinition upgradeItem)
    {
        int currentLevel = DefaultPlayerInventory.Instance.GetItemCount(upgradeItem.ItemID) + 1;
   
        float t = Mathf.Clamp01((float)currentLevel / _maxUpgradeLevel);
        
        float rawCost = _costCurve.Evaluate(t);
        
        int cost = Mathf.RoundToInt(rawCost);
        
        int playerItemCount = DefaultPlayerInventory.Instance.GetItemCount(_paymentItem.ItemID);

        return playerItemCount >= cost;
    }

    public int GetCost(ItemBaseDefinition upgradeItem)
    {
        int currentLevel = DefaultPlayerInventory.Instance.GetItemCount(upgradeItem.ItemID) + 1;
   
        float t = Mathf.Clamp01((float)currentLevel / _maxUpgradeLevel);
        
        float rawCost = _costCurve.Evaluate(t);
        
        int cost = Mathf.RoundToInt(rawCost);

        return cost;
    }
    
}
