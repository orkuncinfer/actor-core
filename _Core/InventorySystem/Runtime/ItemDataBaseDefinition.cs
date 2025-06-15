using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "ItemDataBase", menuName = "Inventory System/Item Database Definition")]
public class ItemDataBaseDefinition : ScriptableObject
{
    public List<ItemDefinition> AllItems;

#if UNITY_EDITOR
    [Button]
    public void FindAllItems()
    {
        // Clear the existing items to avoid duplicates
        AllItems.Clear();

        // Get all ItemDefinition assets stored in the project
        string[] guids = AssetDatabase.FindAssets("t:ItemDefinition"); // Find all assets that are of type ItemDefinition
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(assetPath);
            if (item != null)
            {
                AllItems.Add(item);
            }
        }

        // Optional: Debug to check how many items were loaded
        Debug.Log($"Loaded {AllItems.Count} items into the database.");
    }
#endif
   
}
