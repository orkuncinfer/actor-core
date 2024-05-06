using System.Collections.Generic;
using System.IO;
using Firebase.Database;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "UniqueItemList", menuName = "Inventory System/Items/Unique Item List Definition")]
public class UniqueItemListDefinition : ScriptableObject
{
     public List<ItemDefinition> AllItems;
    [ShowInInspector]public Dictionary<string,UniqueItemData> UniqueItemDictionary = new Dictionary<string, UniqueItemData>();
    private FirebaseDatabase _database;
    
   /* [Button ("Fetch Data(FirebaseDatabase)")]
    public void FetchData()
    {
        _database = FirebaseDatabase.GetInstance("https://templateproject-174cf-default-rtdb.europe-west1.firebasedatabase.app/");

        foreach (var itemDefinition in UniqueItems)
        {
            if(UniqueItemDictionary.ContainsKey(itemDefinition.ID))
                continue;
            MonsterData monsterData = new MonsterData();
            _database.GetReference(CollectionName + "/" + itemDefinition.MonsterID).GetValueAsync().ContinueWith(
                task =>
                {
                    string json = task.Result.GetRawJsonValue();
                    var deserialized = JsonUtility.FromJson(json.ToString(), typeof(MonsterData));
                    monsterData = (MonsterData) deserialized;
                    MonsterDataDictionary.Add(itemDefinition.MonsterID, monsterData);
                });
        }
    }*/
    
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
    [Button]
    public void SaveToDatabase()
    {
        UniqueItemData uniqueItemData = new UniqueItemData();
        var _database = FirebaseDatabase.GetInstance("https://templateproject-174cf-default-rtdb.europe-west1.firebasedatabase.app/");
        foreach (UniqueItemDefinition uniqueItem in AllItems)
        {
            uniqueItemData.ItemID = uniqueItem.ID;
            uniqueItemData.PossibleModifierIDs = new string[uniqueItem.PossibleModifiers.Length];
            for (int i = 0; i < uniqueItem.PossibleModifiers.Length; i++)
            {
                uniqueItemData.PossibleModifierIDs[i] = uniqueItem.PossibleModifiers[i].ModifierID;
            }
            
            _database.GetReference("unique-items/"+uniqueItem.ID).SetRawJsonValueAsync(JsonUtility.ToJson(uniqueItemData,true));
        }
    }
    [Button]
    public void SaveToJson()
    {
        UniqueItemData[] itemModifierData = new UniqueItemData[AllItems.Count];
        for (int i = 0; i < AllItems.Count; i++)
        {
            itemModifierData[i] = new UniqueItemData();
            itemModifierData[i].ItemID = AllItems[i].ID;
            if(AllItems[i] is UniqueItemDefinition uniqueItemDefinition)
            {
                itemModifierData[i].PossibleModifierIDs = new string[uniqueItemDefinition.PossibleModifiers.Length];
                for (int j = 0; j < uniqueItemDefinition.PossibleModifiers.Length; j++)
                {
                    itemModifierData[i].PossibleModifierIDs[j] = uniqueItemDefinition.PossibleModifiers[j].ModifierID;
                }
            }
        }
        
        string json = JsonConvert.SerializeObject(itemModifierData, Formatting.Indented);
        File.WriteAllText(Application.persistentDataPath +"/unique-items.json" , json);
        Application.OpenURL(Application.persistentDataPath);
    }
#endif
}

public class UniqueItemData
{
    public string ItemID;
    public string[] PossibleModifierIDs;
}