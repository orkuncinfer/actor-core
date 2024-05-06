using System.Collections.Generic;
using System.IO;
using Firebase.Database;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemModifierList", menuName = "Inventory System/Modifiers/Item Modifier List Definition")]
public class ItemModifierListDefinition : ScriptableObject
{
    public List<ItemModifierDefinition> AllItemModifiers;
    
#if UNITY_EDITOR
    [Button]
    public void FindAllItems()
    {
        // Clear the existing items to avoid duplicates
        AllItemModifiers.Clear();

        // Get all ItemDefinition assets stored in the project
        string[] guids = AssetDatabase.FindAssets("t:ItemModifierDefinition"); // Find all assets that are of type ItemDefinition
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ItemModifierDefinition item = AssetDatabase.LoadAssetAtPath<ItemModifierDefinition>(assetPath);
            if (item != null)
            {
                AllItemModifiers.Add(item);
            }
        }

        // Optional: Debug to check how many items were loaded
        Debug.Log($"Loaded {AllItemModifiers.Count} items into the database.");
    }
    [Button]
    public void SaveToDatabase()
    {
        ItemModifierData itemModifierData = new ItemModifierData();
        var _database = FirebaseDatabase.GetInstance("https://templateproject-174cf-default-rtdb.europe-west1.firebasedatabase.app/");
        foreach (var definition in AllItemModifiers)
        {
            itemModifierData.ModifierID = definition.ModifierID;
            itemModifierData.ModifierValues = new ItemModifierValue[definition.ModifierValues.Length];
            for (int i = 0; i < definition.ModifierValues.Length; i++)
            {
                itemModifierData.ModifierValues[i] = definition.ModifierValues[i];
            }
            _database.GetReference("item-modifiers/"+ definition.ModifierID).SetRawJsonValueAsync(JsonUtility.ToJson(itemModifierData,true));
        }
    }
    [Button]
    public void SaveToJson()
    {
        ItemModifierData[] itemModifierData = new ItemModifierData[AllItemModifiers.Count];
        for (int i = 0; i < AllItemModifiers.Count; i++)
        {
            itemModifierData[i] = new ItemModifierData();
            itemModifierData[i].ModifierID = AllItemModifiers[i].ModifierID;
            itemModifierData[i].ModifierValues = new ItemModifierValue[AllItemModifiers[i].ModifierValues.Length];
            for (int j = 0; j < AllItemModifiers[i].ModifierValues.Length; j++)
            {
                itemModifierData[i].ModifierValues[j] = AllItemModifiers[i].ModifierValues[j];
            }
        }
        
        string json = JsonConvert.SerializeObject(itemModifierData, Formatting.Indented);
        File.WriteAllText(Application.persistentDataPath +"/item-modifiers.json" , json);
        Application.OpenURL(Application.persistentDataPath);
    }
    
#endif
}