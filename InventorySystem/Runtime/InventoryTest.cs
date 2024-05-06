using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Functions;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    public InventoryDefinition InventoryDefinition;
    
    public ItemDefinition ItemDefinition;
    public int AddCount;
    public List<ItemModifierDefinition> ItemModifierDefinitions;
    public GeneratedItemResult GeneratedItemResult;
    protected FirebaseFunctions functions;
    private void Awake()
    {
        
    }

    [Button]
    public void AddItem()
    {
       
        InventoryDefinition.AddItem(ItemDefinition, AddCount);
    }
    
    [Button]
    public void RemoveItem()
    {
        InventoryDefinition.RemoveItem(ItemDefinition, AddCount);
    }
    [Button]
    public void SaveInventory()
    {
        InventoryDefinition.SaveInventory();
    }
    
     
    [Button]
    public void ContainsItem(ItemDefinition item)
    {
       // DDebug.Log(InventoryDefinition.ContainsItem(item.ID));
    }

    public void CallFunction()
    {
        //GetRandomModifierValues("021012");
        StartCoroutine(GetModifier("item_0002"));
    }
    [Button]
    public void SaveToDatabase()
    {
        ItemModifierData itemModifierData = new ItemModifierData();
        foreach (var definition in ItemModifierDefinitions)
        {
            itemModifierData.ModifierID = definition.ModifierID;
            itemModifierData.ModifierValues = new ItemModifierValue[definition.ModifierValues.Length];
            for (int i = 0; i < definition.ModifierValues.Length; i++)
            {
                itemModifierData.ModifierValues[i] = definition.ModifierValues[i];
            }
        }
        
        
        var _database = FirebaseDatabase.GetInstance("https://templateproject-174cf-default-rtdb.europe-west1.firebasedatabase.app/");
        _database.GetReference("item-modifiers/"+ItemModifierDefinitions[0].ModifierID).SetRawJsonValueAsync(JsonUtility.ToJson(itemModifierData,true));
    }

    public async void GetRandomModifierValues(string modId)
    {
        var functions = FirebaseFunctions.DefaultInstance;
        var data = new Dictionary<string, object>();
        data["itemId"] = modId;
        var function = functions.GetHttpsCallable("getModifier");
        try
        {
            var result = await function.CallAsync(data);
            var jsonData = result.Data.ToString(); // Convert the result data to JSON string
            var response = JsonConvert.DeserializeObject<FunctionResponse>(jsonData);
            //Debug.Log("Random Modifier Values: " + String.Join(", ", response.randomValues));
            Debug.Log("success" + jsonData);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error getting modifier values: " + e.Message);
        }
    }

    [System.Serializable]
    public class ModifierRequest
    {
        public string modifierId;
    }
    [System.Serializable]
    public class FunctionResponse
    {
        public int[] randomValues;
    }
    protected IEnumerator AddNumbers(string firstNumber, int secondNumber) {
        functions = FirebaseFunctions.DefaultInstance;
        var func = functions.GetHttpsCallable("addNumbers");
        var data = new Dictionary<string, object>();
        data["firstNumber"] = firstNumber;
        data["secondNumber"] = secondNumber;
        
        var task = func.CallAsync(data).ContinueWithOnMainThread((callTask) => {
            if (callTask.IsFaulted) {
                // The function unexpectedly failed.
                //DebugLog("FAILED!");
                Debug.Log(("  Error: {0}", callTask.Exception));
                return;
            }

            // The function succeeded.
            var result = (IDictionary)callTask.Result.Data; 
            Debug.Log(("AddNumbers: {0}", result["operationResult"]));
        });
        yield return new WaitUntil(() => task.IsCompleted);
    }
    
    protected IEnumerator GetModifier(string modId) {
        functions = Firebase.Functions.FirebaseFunctions.GetInstance("europe-west3");
        var func = functions.GetHttpsCallable("generateItem");
        var request = new Dictionary<string, object>();
        request.Add("itemId", modId);
     

        var task = func.CallAsync(request).ContinueWithOnMainThread((callTask) => {
            if (callTask.IsFaulted) {
                // The function unexpectedly failed.
                //DebugLog("FAILED!");
                Debug.Log(("  Error: {0}", callTask.Exception));
                return;
            }

            // The function succeeded.
            var result = (IDictionary)callTask.Result.Data;
            string jsonFormat = JsonConvert.SerializeObject(callTask.Result.Data, Formatting.Indented);
            Debug.Log("jsonis " + jsonFormat);
            GeneratedItemResult = JsonConvert.DeserializeObject<GeneratedItemResult>(jsonFormat);
            foreach (List<object> value in result.Values)
            {
                Debug.Log("modifier value is "+ value[0]);
            }
            Debug.Log(("AddNumbers: {0}", result.Contains("randomValues[0]")));
            foreach(KeyValuePair<object,object> attachStat in result)
            {
                //Now you can access the key and value both separately from this attachStat as:
                Debug.Log(attachStat.Key);
                Debug.Log(attachStat.Value);
            }
        });
        yield return new WaitUntil(() => task.IsCompleted);
    }
}
