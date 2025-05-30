using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UI_InventoryView : MonoBehaviour
{
    [SerializeField] private InventoryDefinition _inventoryDefinition;
    [SerializeField] private GridLayoutGroup _inventoryLayout;
    [FormerlySerializedAs("_uıItemElementPrefab")] [FormerlySerializedAs("ItemElementPrefab")] [SerializeField] private UI_InventoryItemElement _uıInventoryItemElementPrefab;

    [SerializeField] private GameObject _garbageDragArea;

    [SerializeField] private EventField<InventoryActionArgs> _inventoryActionEvent;
    

  

    private Dictionary<int, UI_InventoryItemElement> _itemElements = new Dictionary<int, UI_InventoryItemElement>();

    private InventoryDefinition _lastBuiltInventory;

    public bool IsInitialized { get; private set; }
   
    
    public void SetInventoryDefinition(InventoryDefinition inventoryDefinition)
    {
        _inventoryDefinition = inventoryDefinition;
        BuildSlots();
        OnInventoryChanged();
    }

    private void OnInventoryChanged()
    {
        foreach (var item in _itemElements)
        {
            item.Value.ClearItemData();
        }

        for (int i = 0; i < _inventoryDefinition.InventoryData.InventorySlots.Count; i++)
        {
            if (_inventoryDefinition.InventoryData.InventorySlots[i].ItemData == null)
            {
                continue;
            }
            
            _itemElements[i].SetItemData(_inventoryDefinition.InventoryData.InventorySlots[i].ItemID,
                _inventoryDefinition.InventoryData.InventorySlots[i]);
        }
    }

    private void Start()
    {
        BuildSlots();
    }

    public void BuildSlots()
    {
        if(_inventoryDefinition == null) return;

        if (_lastBuiltInventory != null)
        {
            for (int i = 0; i < _itemElements.Count; i++)
            {
                if (_itemElements[i] != null)
                {
                    Destroy(_itemElements[i].gameObject);
                }
            }
            _itemElements.Clear();
        }
        
        for (int i = 0; i < _inventoryDefinition.InventoryData.InventorySlots.Count; i++)
        {
            UI_InventoryItemElement uıInventoryItemElement = Instantiate(_uıInventoryItemElementPrefab, _inventoryLayout.transform);
            _itemElements.Add(i, uıInventoryItemElement);
            if (_inventoryDefinition.InventoryData.InventorySlots[i].ItemData != null)
            {
                uıInventoryItemElement.SetItemData(_inventoryDefinition.InventoryData.InventorySlots[i].ItemID,
                    _inventoryDefinition.InventoryData.InventorySlots[i]);
            }
            else
            {
                uıInventoryItemElement.ItemDefinition = null;
            }
            uıInventoryItemElement.OwnerInventory = _inventoryDefinition;
            uıInventoryItemElement.SlotIndex = i;
        }

        _lastBuiltInventory = _inventoryDefinition;
        _inventoryDefinition.onInventoryChanged += OnInventoryChanged;
    }

    
}