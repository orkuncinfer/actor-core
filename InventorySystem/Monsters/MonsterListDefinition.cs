using System.Collections;
using System.Collections.Generic;
using System.IO;
using Firebase.Database;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterList_", menuName = "Inventory System/Monster/Monster List Definition")]
public class MonsterListDefinition : ScriptableObject
{
    public List<MonsterDefinition> AllMonsters;
    public string CollectionName = "monsters";
    [ShowInInspector]public Dictionary<string,MonsterData> MonsterDataDictionary = new Dictionary<string, MonsterData>();
    
    private FirebaseDatabase _database;

    [Button ("Fetch Data(FirebaseDatabase)")]
    public void FetchData()
    {
        _database = FirebaseDatabase.GetInstance("https://templateproject-174cf-default-rtdb.europe-west1.firebasedatabase.app/");

        foreach (var monsterDefinition in AllMonsters)
        {
            if(MonsterDataDictionary.ContainsKey(monsterDefinition.MonsterID))
                continue;
            MonsterData monsterData = new MonsterData();
            _database.GetReference(CollectionName + "/" + monsterDefinition.MonsterID).GetValueAsync().ContinueWith(
                task =>
                {
                    string json = task.Result.GetRawJsonValue();
                    var deserialized = JsonUtility.FromJson(json.ToString(), typeof(MonsterData));
                    monsterData = (MonsterData) deserialized;
                    MonsterDataDictionary.Add(monsterDefinition.MonsterID, monsterData);
                });
        }
    }
    
#if UNITY_EDITOR
    [Button]
    public void FindAllItems()
    {
        // Clear the existing items to avoid duplicates
        AllMonsters.Clear();

        // Get all ItemDefinition assets stored in the project
        string[] guids = AssetDatabase.FindAssets("t:MonsterDefinition"); // Find all assets that are of type ItemDefinition
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            MonsterDefinition item = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(assetPath);
            if (item != null)
            {
                AllMonsters.Add(item);
            }
        }

        // Optional: Debug to check how many items were loaded
        Debug.Log($"Loaded {AllMonsters.Count} items into the database.");
    }
    [Button]
    public void SaveToDatabase()
    {
        MonsterData monsterData = new MonsterData();
        var _database = FirebaseDatabase.GetInstance("https://templateproject-174cf-default-rtdb.europe-west1.firebasedatabase.app/");
        
        foreach (MonsterDefinition monster in AllMonsters)
        {
            monsterData.MonsterId = monster.MonsterID;
            //monsterData.PossibleDrops = monster.PossibleDrops;

            for (int i = 0; i < monsterData.PossibleDrops.Length; i++)
            {
                monsterData.PossibleDrops[i].ItemId = monster.PossibleDrops[i].Item.ID;
                //onsterData.PossibleDrops[i].DropChance = Mathf.Round(monsterData.PossibleDrops[i].DropChance * 100f) / 100f;
            }
            
            _database.GetReference(CollectionName+"/"+monster.MonsterID).SetRawJsonValueAsync(JsonUtility.ToJson(monsterData,true));
        }
    }
    [Button]
    public void SaveToJson()
    {
        MonsterData[] monsterData = new MonsterData[AllMonsters.Count];
        for (int i = 0; i < AllMonsters.Count; i++)
        {
            monsterData[i] = new MonsterData();
            monsterData[i].MonsterId = AllMonsters[i].MonsterID;
            monsterData[i].PossibleDrops = new ItemDropData[AllMonsters[i].PossibleDrops.Length];
            for (int j = 0; j < AllMonsters[i].PossibleDrops.Length; j++)
            {
                monsterData[i].PossibleDrops[j] = new ItemDropData();
                monsterData[i].PossibleDrops[j].ItemId = AllMonsters[i].PossibleDrops[j].Item.ID;
                monsterData[i].PossibleDrops[j].DropChance = AllMonsters[i].PossibleDrops[j].DropChance;
            }
        }

        string json = JsonConvert.SerializeObject(monsterData, Formatting.Indented);
        File.WriteAllText(Application.persistentDataPath +"/" + CollectionName, json);
        Application.OpenURL(Application.persistentDataPath);
    }
    
    public void OpenSavedPath()
    {
        Application.OpenURL(Application.persistentDataPath);
    }
#endif
}

public class MonsterData
{
    public string MonsterId;
    public ItemDropData[] PossibleDrops;
}

public class ItemDropData
{
    public string ItemId;
    public float DropChance;
}
