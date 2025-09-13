using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using FilePathAttribute = UnityEditor.FilePathAttribute;

[SOCreatable("Items")]
public class ItemListDefinition : ScriptableObject
{
    public List<ItemBaseDefinition> AllItems = new List<ItemBaseDefinition>();
    
    private Dictionary<string,ItemBaseDefinition> _itemDictionary = new Dictionary<string, ItemBaseDefinition>();

    public string TypeName;

    [FolderPath]public string Location;
    
    public ItemBaseDefinition GetItem(string itemId)
    {
        if (_itemDictionary.Count == 0)
        {
            foreach (var itemDefinition in AllItems)
            {
                if (_itemDictionary.ContainsKey(itemDefinition.ItemId))
                {
                    Debug.LogError($"Item with id {itemDefinition.ItemId} already exists in the dictionary. Duplicate item found: {itemDefinition.name}");
                }
                _itemDictionary.Add(itemDefinition.ItemId, itemDefinition);
            }
        }
        if (_itemDictionary.TryGetValue(itemId, out ItemBaseDefinition item))
        {
            return item;
        }
        Debug.Log($"Couldn't found the item with id : " + itemId);
        return null;
    }
    
#if UNITY_EDITOR
    [Button]
    public void FindAllItems()
    {
        // Clear the existing items to avoid duplicates
        AllItems.Clear();

        // Get all ItemDefinition assets stored in the project
        string[] guids = AssetDatabase.FindAssets($"t:{TypeName}"); // Find all assets of the specified type

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // Load the main asset
            ItemBaseDefinition mainItem = AssetDatabase.LoadAssetAtPath<ItemBaseDefinition>(assetPath);
            if (mainItem != null)
            {
                if(!AllItems.Contains(mainItem))
                    AllItems.Add(mainItem);
            }

            // Load all assets at the same path (including sub-assets)
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (Object asset in allAssets)
            {
                if (asset == null || asset == mainItem)
                    continue;

                if (asset is ItemBaseDefinition childItem)
                {
                    if(!AllItems.Contains(childItem))
                        AllItems.Add(childItem);
                }
            }
        }

        if (!Location.IsNullOrWhitespace())
        {
            
        }
    }

    [Button]
    public void GenerateIdForAll()
    {
        foreach (var itemDefinition in AllItems)
        {
            if (string.IsNullOrEmpty(itemDefinition.ItemId))
            {
                itemDefinition.SetId();
            }
        }
        AssetDatabase.SaveAssets();
    }
    #endif
}
