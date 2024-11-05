using TMPro;
using UnityEngine;

public class UI_WeaponHud : UI_EquipmentItemElement
{
    [SerializeField] private TextMeshProUGUI _ammoText;
    public override void SetItemData(string itemID, InventorySlot slotData)
    {
        base.SetItemData(itemID, slotData);
        
        
    }
}