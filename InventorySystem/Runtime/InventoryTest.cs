using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
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
}
