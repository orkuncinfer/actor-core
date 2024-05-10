using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using UnityEditor;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Firestore;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine.Serialization;
using Formatting = System.Xml.Formatting;


[CreateAssetMenu(fileName = "New Inventory Definition", menuName = "Inventory System/Inventory Definition")]
public class InventoryDefinition : ScriptableObject
{

    public int InitialSlotCount;
    public int Width;
    public string InventoryId;
    
    public InventoryData InventoryData = new InventoryData();

   public event Action onInventoryChanged;

    private string savePath => "/" + InventoryId;
    private FirebaseDatabase _database;
    private FirebaseFirestore _firestore;   

    [Button]
    public void Initialize()
    {
        InventoryData = new InventoryData();
        if(InventoryData.InventorySlots == null || InventoryData.InventorySlots.Count == 0)
        {
            InventoryData.InventorySlots = new List<InventorySlot>();
            for (int i = 0; i < InitialSlotCount; i++)
            {
                InventorySlot inventorySlot = new InventorySlot();
                InventoryData.InventorySlots.Add(inventorySlot);
            }
        }
        _database = FirebaseDatabase.GetInstance("https://templateproject-174cf-default-rtdb.europe-west1.firebasedatabase.app/");
        _firestore = FirebaseFirestore.DefaultInstance;
    }
    public void AddItem(ItemDefinition itemDefinition, int count)
    {
        for (int i = 0; i < InventoryData.InventorySlots.Count; i++) // check if we already have same item
        {
            if (InventoryData.InventorySlots[i].ItemID == itemDefinition.ID)
            {
                if(InventoryData.InventorySlots[i].ItemCount < itemDefinition.MaxStack)
                {
                    int emptyCount = itemDefinition.MaxStack - InventoryData.InventorySlots[i].ItemCount;
                    if (emptyCount >= count)
                    {
                        DefineItemToSlot(itemDefinition, count, InventoryData.InventorySlots[i]);
                        return;
                    }
                    if (emptyCount < count)
                    {
                        DefineItemToSlot(itemDefinition, emptyCount, InventoryData.InventorySlots[i]);
                        AddItem(itemDefinition,count-emptyCount);
                        return;
                    }
                }
            }
        }

        for (int i = 0; i < InventoryData.InventorySlots.Count; i++)
        {
            //if(InventoryData.InventorySlots[i].ReachedMaxStack) continue;
            if (InventoryData.InventorySlots[i].AllowedTypes.Length != 0) // allowed type filter
            {
                bool allowed = false;
                for (int j = 0; j < InventoryData.InventorySlots[i].AllowedTypes.Length; j++)
                {
                    if (InventoryData.InventorySlots[i].AllowedTypes[j] == itemDefinition.ItemType)
                    {
                        allowed = true;
                        break;
                    }   
                }
                if(!allowed) continue;
            }
            
            if (InventoryData.InventorySlots[i].AllowedTypes.Length == 0)
            {
                int emptyCount = itemDefinition.MaxStack - InventoryData.InventorySlots[i].ItemCount;
                if (!InventoryData.InventorySlots[i].ItemID.IsNullOrWhitespace())
                {
                    if(itemDefinition.ID != InventoryData.InventorySlots[i].ItemID) continue;
                }
               
                if(emptyCount > 0 && emptyCount >= count)
                {
                    DefineItemToSlot(itemDefinition, count, InventoryData.InventorySlots[i]);
                    return;
                }
                if (emptyCount > 0 && emptyCount < count)
                {
                    DefineItemToSlot(itemDefinition, emptyCount, InventoryData.InventorySlots[i]);
                    //InventoryData.InventorySlots[i].ReachedMaxStack = true;
                    AddItem(itemDefinition,count-emptyCount);
                    return;
                }
                continue;
            }
        }
        Debug.Log("inventory is full");
    }

    public bool ContainsItem(ItemDefinition itemDefinition, int count)
    {
        int availableCount = 0;
  
        foreach (var slot in InventoryData.InventorySlots)
        {
            if (slot.ItemID == itemDefinition.ID)
            {
                availableCount += slot.ItemCount;
            }
        }
        if (availableCount < count)
        {
            Debug.Log("Not enough items in inventory to remove.");
            return false; 
        }

        return true;
    }
    
    public void RemoveItem(ItemDefinition itemDefinition, int count)
    {
        if (count <= 0) return; 

        int availableCount = 0;
        foreach (var slot in InventoryData.InventorySlots)
        {
            if (slot.ItemID == itemDefinition.ID)
            {
                availableCount += slot.ItemCount;
            }
        }
        // Check if there are enough items to remove
        if (availableCount < count)
        {
            Debug.Log("Not enough items in inventory to remove.");
            return; 
        }
        // Second pass: remove items
        for (int i = InventoryData.InventorySlots.Count - 1; i >= 0; i--)
        {
            if (InventoryData.InventorySlots[i].ItemID == itemDefinition.ID)
            {
                if (InventoryData.InventorySlots[i].ItemCount > count)
                {
                    InventoryData.InventorySlots[i].ItemCount -= count;
                    break; // Completed the removal requirement
                }
                else
                {
                    count -= InventoryData.InventorySlots[i].ItemCount; 
                    InventoryData.InventorySlots[i].ItemCount = 0;
                    RemoveEntireSlot(InventoryData.InventorySlots[i]);
                    //ResetItemData(InventoryData.InventorySlots[i].ItemData); // Reset the data as this slot is now empty*******????????
                }
            }
        }
    }
    
    public void RemoveEntireSlot(InventorySlot slot)
    {
        slot.ItemID = null;
        slot.ItemCount = 0;
    }
    public void SwapOrMergeItems(int index1, int index2)
    {
        if (InventoryData.InventorySlots[index1].ItemID == InventoryData.InventorySlots[index2].ItemID)
        {
            ItemDefinition itemDefinition = InventoryUtils.FindItemWithId(InventoryData.InventorySlots[index1].ItemID);
            if (itemDefinition.IsStackable)
            {
                int emptyCount = itemDefinition.MaxStack - InventoryData.InventorySlots[index2].ItemCount;
                if (emptyCount > 0)
                {
                    if (emptyCount >= InventoryData.InventorySlots[index1].ItemCount)
                    {
                        InventoryData.InventorySlots[index2].ItemCount += InventoryData.InventorySlots[index1].ItemCount;
                        RemoveEntireSlot(InventoryData.InventorySlots[index1]);
                        onInventoryChanged?.Invoke();
                        return;
                    }
                    else
                    {
                        InventoryData.InventorySlots[index1].ItemCount -= emptyCount;
                        InventoryData.InventorySlots[index2].ItemCount += emptyCount;
                        onInventoryChanged?.Invoke();
                        return;
                    }
                }
            }
        }
        
        (InventoryData.InventorySlots[index1], InventoryData.InventorySlots[index2]) = (InventoryData.InventorySlots[index2], InventoryData.InventorySlots[index1]);
        onInventoryChanged?.Invoke();
    }
    
    void ResetItemData(ItemData itemData)
    {
        itemData.ItemID = "";
        itemData.Name = "";
        itemData.UpgradeLevel = 0;
        itemData.Attributes = new ItemAttribute[0];
        onInventoryChanged?.Invoke();
    }
    private void DefineItemToSlot(ItemDefinition itemDefinition, int count, InventorySlot slot)
    {
        slot.ItemCount += count;
        slot.ItemID = itemDefinition.ID;
        //slot.ItemData.Name = itemDefinition.ItemName;
        //TODO: security validation here
        SaveData();
        onInventoryChanged?.Invoke();
    }


    #region Save/Load

    [Button]
    public void SaveInventory()
    {
        //string json = JsonConvert.SerializeObject(InventorySlots, Formatting.Indented);
        //File.WriteAllText(Application.persistentDataPath + savePath, json);
        SaveData();
    }

    public void SetInventoryData(InventoryData data)
    {
        InventoryData = data;
        onInventoryChanged?.Invoke();
    }
    
    public async void  SaveData()
    {
        //string json = JsonConvert.SerializeObject(InventoryData);
       
        //Debug.Log("data is " + json);
        var playerData = new Dictionary<string, object>{    
            {InventoryId, InventoryData},
        };
 
        
        await _firestore.Collection("player-data").Document(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Collection("Inventories").Document(InventoryId).SetAsync(InventoryData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SaveAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SaveAsync encountered an error: " + task.Exception);
                return;
            }
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("SaveAsync completed successfully.");
            }
        });
        //await _database.GetReference(GooglePlayServicesInitialization.Instance.ThisUser.UserId + "/MainInventory").SetRawJsonValueAsync(JsonUtility.ToJson(InventoryData,true));
        //_database.GetReference(GooglePlayServicesInitialization.Instance.ThisUser.UserId + "/MainInventory").SetRawJsonValueAsync(JsonUtility.ToJson(InventoryData,true));

        //CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
        /*try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
            Debug.Log($"Data saved successfully: {string.Join(", ", playerData.Select(kv => $"{kv.Key}={kv.Value}"))}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save data: {ex.Message}");
        }
        Debug.Log($"Saved data {string.Join(',', playerData)}");*/
    }

    #endregion
   
}