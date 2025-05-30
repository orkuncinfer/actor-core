using System;
using System.Collections;
using System.Collections.Generic;
using Flexalon;
using TMPro;
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
    [SerializeField] private Button _closeButton;

    [SerializeField] private TMP_InputField _inventorySearchInput;
    [SerializeField] private TMP_InputField _itemListSearchInput;

    private List<GameObject> _spawnedInventoryItems = new List<GameObject>();
    private List<GameObject> _spawnedItemListItems = new List<GameObject>();

    private void OnEnable()
    {
        _closeButton.onClick.AddListener(OnClosePanel);
        _inventoryButton.onClick.AddListener(OnInventoryButtonClicked);
        _itemListButton.onClick.AddListener(OnItemListButtonClicked);
        _inventorySearchInput.onValueChanged.AddListener(FilterInventoryItems);
        _itemListSearchInput.onValueChanged.AddListener(FilterItemListItems);
        DefaultPlayerInventory.Instance.onItemChanged += OnItemChanged;

        PopulateInventory();
        OnItemListButtonClicked();
        OnInventoryButtonClicked();
        _inventoryButton.Select();
    }

    private void OnClosePanel()
    {
        transform.parent.gameObject.SetActive(false);
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

        if (_spawnedItemListItems.Count == 0)
            PopulateItemList();
    }

    private void OnInventoryButtonClicked()
    {
        _inventoryPanel.SetActive(true);
        _itemListPanel.SetActive(false);

        PopulateInventory();
    }

    public void PopulateInventory()
    {
        if (_inventoryPanel.activeInHierarchy == false) return;
        GameObject[] itemsToDestroy = _spawnedInventoryItems.ToArray();
        foreach (GameObject spawnedInventoryItem in itemsToDestroy)
        {
            if (spawnedInventoryItem == _inventoryItemReference) continue;
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
            if (spawnedInventoryItem == _listItemReference) continue;
            Destroy(spawnedInventoryItem);
        }

        _spawnedItemListItems.Clear();

        _listItemReference.SetActive(false);
        int index = 0;
        var itemList = InventoryUtils.GetItemsList();
        foreach (var item in itemList)
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

            instance.name = itemDefinition.name;
            _spawnedItemListItems.Add(instance);
            index++;
        }
    }

    #region SEARCH

    private void FilterInventoryItems(string searchText)
    {
        foreach (var item in _spawnedInventoryItems)
        {
            bool isMatch = IsFuzzyMatch(item.name, searchText);
            item.SetActive(isMatch);
        }
    }

    private void FilterItemListItems(string searchText)
    {
        foreach (var item in _spawnedItemListItems)
        {
            bool isMatch = IsFuzzyMatch(item.name, searchText);
            item.SetActive(isMatch);
        }
    }

    private bool IsFuzzyMatch(string itemName, string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return true;

        string normalizedItemName = itemName.ToLower();
        string normalizedSearchText = searchText.ToLower();

        string itemNameWithSpaces = normalizedItemName.Replace('_', ' ');
        string searchTextWithUnderscores = normalizedSearchText.Replace(' ', '_');

        // Direct containment check (fast path before using Levenshtein distance)
        if (itemNameWithSpaces.Contains(normalizedSearchText) ||
            normalizedItemName.Contains(searchTextWithUnderscores))
            return true;

        int distance1 = LevenshteinDistance(itemNameWithSpaces, normalizedSearchText);
        int distance2 = LevenshteinDistance(normalizedItemName, searchTextWithUnderscores);

        return distance1 <= Mathf.Max(1, searchText.Length / 3) ||
               distance2 <= Mathf.Max(1, searchText.Length / 3);
    }

    private int LevenshteinDistance(string s1, string s2)
    {
        int len1 = s1.Length;
        int len2 = s2.Length;
        int[,] dp = new int[len1 + 1, len2 + 1];

        for (int i = 0; i <= len1; i++) dp[i, 0] = i;
        for (int j = 0; j <= len2; j++) dp[0, j] = j;

        for (int i = 1; i <= len1; i++)
        {
            for (int j = 1; j <= len2; j++)
            {
                int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                dp[i, j] = Mathf.Min(dp[i - 1, j] + 1,
                    dp[i, j - 1] + 1,
                    dp[i - 1, j - 1] + cost);
            }
        }

        return dp[len1, len2];
    }

    #endregion
}