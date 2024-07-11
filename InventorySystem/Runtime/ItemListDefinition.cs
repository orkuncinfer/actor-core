using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
[SOCreatable]
public class ItemListDefinition : ScriptableObject
{
    public List<ItemBaseDefinition> AllItems = new List<ItemBaseDefinition>();
    
    private Dictionary<string,ItemBaseDefinition> _itemDictionary = new Dictionary<string, ItemBaseDefinition>();
    
    public ItemBaseDefinition GetItem(string itemId)
    {
        if (_itemDictionary.Count == 0)
        {
            foreach (var itemDefinition in AllItems)
            {
                _itemDictionary.Add(itemDefinition.ItemId, itemDefinition);
            }
        }
        if (_itemDictionary.TryGetValue(itemId, out ItemBaseDefinition item))
        {
            return item;
        }
        return null;
    }
    
#if UNITY_EDITOR
    [Button]
    public void FindAllItems()
    {
        // Clear the existing items to avoid duplicates
        AllItems.Clear();

        // Get all ItemDefinition assets stored in the project
        string[] guids = AssetDatabase.FindAssets("t:ItemBaseDefinition"); // Find all assets that are of type ItemDefinition
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ItemBaseDefinition item = AssetDatabase.LoadAssetAtPath<ItemBaseDefinition>(assetPath);
            if (item != null)
            {
                AllItems.Add(item);
            }
        }
    }
    #endif
}
