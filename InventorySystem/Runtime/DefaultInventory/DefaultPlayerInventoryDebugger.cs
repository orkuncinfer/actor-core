using System;
using System.Collections;
using System.Collections.Generic;
using Flexalon;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DefaultPlayerInventoryDebugger : MonoBehaviour
{
    [SerializeField] private Button _inventoryButton;
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private Button _itemListButton;
    [SerializeField] private GameObject _itemListPanel;
    [SerializeField] private GameObject _inventorySpawnContent;
    [SerializeField] private GameObject _inventoryItemReference;
    [SerializeField] private GameObject _itemListSpawnContent;
    [SerializeField] private GameObject _listItemReference;
    
    private List<GameObject> _spawnedInventoryItems = new List<GameObject>();
    private List<GameObject> _spawnedItemListItems = new List<GameObject>();

    private void OnEnable()
    {
        _inventoryButton.onClick.AddListener(OnInventoryButtonClicked);
        _itemListButton.onClick.AddListener(OnItemListButtonClicked);
        DefaultPlayerInventory.Instance.onItemChanged += OnItemChanged;
        
        PopulateInventory();
        OnItemListButtonClicked();
        OnInventoryButtonClicked();
    }

    private void OnDestroy()
    {
        _inventoryButton.onClick.RemoveListener(OnInventoryButtonClicked);
        _itemListButton.onClick.RemoveListener(OnItemListButtonClicked);
        DefaultPlayerInventory.Instance.onItemChanged -= OnItemChanged;
    }

    private void OnItemChanged(string arg1, int arg2, int arg3)
    {
        PopulateInventory();
    }

    private void OnItemListButtonClicked()
    {
        _itemListPanel.SetActive(true);
        _inventoryPanel.SetActive(false);
        _itemListPanel.GetComponent<FlexalonObject>().SkipLayout = false;
        _inventoryPanel.GetComponent<FlexalonObject>().SkipLayout = true;
        
        PopulateItemList();
    }

    private void OnInventoryButtonClicked()
    {
        _inventoryPanel.SetActive(true);
        _itemListPanel.SetActive(false);
        _itemListPanel.GetComponent<FlexalonObject>().SkipLayout = true;
        _inventoryPanel.GetComponent<FlexalonObject>().SkipLayout = false;
        
        PopulateInventory();
    }

    public void PopulateInventory()
    {
        if(_inventoryPanel.activeInHierarchy == false) return;
        GameObject[] itemsToDestroy = _spawnedInventoryItems.ToArray();
        foreach (GameObject spawnedInventoryItem in itemsToDestroy)
        {
            if(spawnedInventoryItem == _inventoryItemReference) continue;
            Destroy(spawnedInventoryItem);
        }
        _spawnedInventoryItems.Clear();
        
        _inventoryItemReference.SetActive(false);
        int index = 0;
        var inventory = DefaultPlayerInventory.Instance.GetInventory();
        foreach (var item in inventory)
        { 
            ItemBaseDefinition itemDefinition = DefaultPlayerInventory.Instance.GetItemDefinition(item.Key);
            GameObject instance = Instantiate(_inventoryItemReference, _inventorySpawnContent.transform);
            if (index == 0)
            {
                Destroy(_inventoryItemReference);
                _inventoryItemReference = instance;
            }
            instance.SetActive(true);
            instance.GetComponentInChildren<InventoryDebuggerItemDisplay>().ItemDefinition = itemDefinition;
            instance.GetComponentInChildren<InventoryDebuggerItemDisplay>().DisplayItem(itemDefinition);
            
            _spawnedInventoryItems.Add(instance);
            index++;
        }
    }
    
    public void PopulateItemList()
    {
        foreach (GameObject spawnedInventoryItem in _spawnedItemListItems)
        {
            if(spawnedInventoryItem == _listItemReference) continue;
            Destroy(spawnedInventoryItem);
        }
        _spawnedItemListItems.Clear();
        
        _listItemReference.SetActive(false);
        int index = 0;
        var itemList = DefaultPlayerInventory.Instance.GetAllItemsList();
        foreach (var item in itemList.AllItems)
        { 
            ItemBaseDefinition itemDefinition = DefaultPlayerInventory.Instance.GetItemDefinition(item.ItemId);
            GameObject instance = Instantiate(_listItemReference, _itemListSpawnContent.transform);
            if (index == 0)
            {
                Destroy(_listItemReference);
                _listItemReference = instance;
            }
            
            instance.SetActive(true);
            instance.GetComponentInChildren<InventoryDebuggerItemDisplay>().ItemDefinition = itemDefinition;
            instance.GetComponentInChildren<InventoryDebuggerItemDisplay>().DisplayItem(itemDefinition);
            
            _spawnedItemListItems.Add(instance);
            index++;
        }
    }
}
