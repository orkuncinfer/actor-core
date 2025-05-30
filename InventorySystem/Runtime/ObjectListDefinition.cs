using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[SOCreatable("Items")]
public class ObjectListDefinition : ScriptableObject
{
    public List<object> AllItems = new List<object>();
    
    private Dictionary<string,object> _itemDictionary = new Dictionary<string, object>();
    
    public object GetItem(string itemId)
    {
        if (_itemDictionary.Count == 0)
        {
            foreach (var itemDefinition in AllItems)
            {
                if(itemDefinition is ItemBaseDefinition itemBase)
                _itemDictionary.Add(itemBase.ItemId, itemDefinition);
            }
        }
        if (_itemDictionary.TryGetValue(itemId, out object item))
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