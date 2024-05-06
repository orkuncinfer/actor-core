using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemElement_UI : MonoBehaviour
{   
    [SerializeField] private Image _itemIcon;
    [SerializeField] private Image _rarityBadge;
    [SerializeField] private TextMeshProUGUI _itemCount;
    
    public ItemDefinition ItemDefinition;
    
    public string ItemID;
    
    public int SlotIndex;
    public void SetItemData(string itemID, InventorySlot slotData)
    {
        ItemDefinition = InventoryUtils.FindItemWithId(itemID);
        _itemIcon.sprite = ItemDefinition.Icon;
        _itemIcon.color = Color.white;
        _itemCount.text = slotData.ItemCount.ToString();
        
        Sprite rarityBadge = InventoryUtils.GetRarityBadge(ItemDefinition.DefaultRarity);
        _rarityBadge.sprite = rarityBadge;
        _rarityBadge.gameObject.SetActive(true);
        if (slotData.ItemCount > 1)
        {
            _itemCount.gameObject.SetActive(true);
        }
        else
        {
            _itemCount.gameObject.SetActive(false);
        }
    }
    public void ClearItemData()
    {
        ItemDefinition = null;
        _itemIcon.sprite = null;
        _itemIcon.color = Color.clear;
        _itemCount.text = "";
        _itemCount.gameObject.SetActive(false);
        _rarityBadge.sprite = null;
        _rarityBadge.gameObject.SetActive(false);
    }
}
 