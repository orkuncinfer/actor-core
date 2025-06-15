using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryDebuggerItemDisplay : MonoBehaviour
{
    [ReadOnly] public ItemBaseDefinition ItemDefinition;
    [HideInInspector] public string ItemId;
    [HideInInspector] public int ItemCount;
    [HideInInspector] public Sprite ItemIcon;

    [SerializeField] private TextMeshProUGUI _itemIdText;
    [SerializeField] private TextMeshProUGUI _itemCountText;
    [SerializeField] TMP_InputField _addItemInput;
    [SerializeField] Button _addItem;
    [SerializeField] Button _removeItem;

    private void OnEnable()
    {
        if(_addItem)_addItem.onClick.AddListener(OnAddItem);
        if(_removeItem)_removeItem.onClick.AddListener(OnRemoveItem);
    }

    private void OnRemoveItem()
    {
        int amount = DefaultPlayerInventory.Instance.GetItemCount(ItemDefinition.ItemId);
        DefaultPlayerInventory.Instance.RemoveItem(ItemDefinition.ItemId, amount);
    }

    private void OnAddItem()
    {
        int amount = int.Parse(_addItemInput.text);
        DefaultPlayerInventory.Instance.AddItem(ItemId, amount);
    }

    public void DisplayItem(ItemBaseDefinition definition)
    {
        ItemDefinition = definition;
        ItemIcon = ItemDefinition.Icon;
        ItemId = ItemDefinition.ItemId;
        ItemCount = DefaultPlayerInventory.Instance.GetItemCount(ItemId);
        
        if(_itemIdText)_itemIdText.text = ItemId;
        if(_itemCountText)_itemCountText.text = ItemCount.ToString();
    }
}