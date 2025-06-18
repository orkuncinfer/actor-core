using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_EquipmentItemElement : UI_ItemElement
{
    [SerializeField] private Image _itemIcon;
    [SerializeField] private Image _rarityBadge;
    [SerializeField] private TextMeshProUGUI _itemCount;
    [SerializeField] private TextMeshProUGUI _itemName;
    
    [ReadOnly]public ItemDefinition ItemDefinition;
    
    public override void SetItemData(string itemID, InventorySlot slotData)
    {
        base.SetItemData(itemID, slotData);
        ItemDefinition = InventoryUtils.FindItemDefinitionWithId(itemID);
        if(_itemIcon)_itemIcon.sprite = ItemDefinition.Icon;
        if(_itemIcon)_itemIcon.color = Color.white;
        if(_itemCount)_itemCount.text = slotData.ItemCount.ToString();
        if(_itemName) _itemName.text = ItemDefinition.ItemName;
        Sprite rarityBadge = InventoryUtils.GetRarityBadge(ItemDefinition.DefaultRarity);
        if(_rarityBadge)_rarityBadge.sprite = rarityBadge;
        if(_rarityBadge)_rarityBadge.gameObject.SetActive(true);
        
        if (_itemCount)
        {
            if (slotData.ItemCount > 1)
            {
                _itemCount.gameObject.SetActive(true);
            }
            else
            {
                _itemCount.gameObject.SetActive(false);
            }
        }
    }

    public override void ClearItemData()
    {
        base.ClearItemData();
        ItemDefinition = null;
        if(_itemIcon)_itemIcon.sprite = null;
        if(_itemIcon)_itemIcon.color = Color.clear;
        if(_itemCount)_itemCount.text = "";
        if(_itemCount) _itemCount.gameObject.SetActive(false);
        if(_rarityBadge)_rarityBadge.sprite = null;
        if(_rarityBadge)_rarityBadge.gameObject.SetActive(false);
    }
}
