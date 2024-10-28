using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryItemElement : UI_ItemElement
{   
    [SerializeField] private Image _itemIcon;
    [SerializeField] private Image _rarityBadge;
    [SerializeField] private TextMeshProUGUI _itemCount;
    
    public ItemDefinition ItemDefinition;
    
    public string ItemID;
    
    public int SlotIndex;
    public override void SetItemData(string itemID, InventorySlot slotData)
    {
        base.SetItemData(itemID, slotData);
        ItemDefinition = InventoryUtils.FindItemWithId(itemID);
        if(_itemIcon)_itemIcon.sprite = ItemDefinition.Icon;
        if(_itemIcon)_itemIcon.color = Color.white;
        if(_itemCount)_itemCount.text = slotData.ItemCount.ToString();
        
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
        _itemIcon.sprite = null;
        _itemIcon.color = Color.clear;
        _itemCount.text = "";
        _itemCount.gameObject.SetActive(false);
        _rarityBadge.sprite = null;
        _rarityBadge.gameObject.SetActive(false);
    }
}
 