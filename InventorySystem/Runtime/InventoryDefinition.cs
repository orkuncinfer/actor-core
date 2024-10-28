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
using BandoWare.GameplayTags;
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
public class InventoryDefinition : MonoBehaviour
{

    public int InitialSlotCount;
    public int Width;
    public string InventoryId;

    public bool CreateOnInitialize = false;
    
    public InventoryData InventoryData = new InventoryData();
    
    public ItemPack[] InitialItems;

   public event Action onInventoryChanged;

    private string savePath => "/" + InventoryId;
    private FirebaseDatabase _database;
    private FirebaseFirestore _firestore;

    private void Awake()
    {
        Initialize();
    }

    [Button]
    public void Initialize()
    {
        if (CreateOnInitialize)
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
        }
        foreach (var itemPack in InitialItems)
        {
            if (itemPack.ItemDefinition != null)
            {
                Debug.Log("added item " + itemPack.ItemDefinition.ItemName + " to inventory");
                AddItem(itemPack.ItemDefinition, itemPack.Count);
            }
        }
        _database = FirebaseDatabase.GetInstance("https://templateproject-174cf-default-rtdb.europe-west1.firebasedatabase.app/");
        _firestore = FirebaseFirestore.DefaultInstance;
    }
    [Button]
    public int AddItem(ItemDefinition itemDefinition, int count)
    {
        int totalAdded = 0;

        for (int i = 0; i < InventoryData.InventorySlots.Count; i++)
        {
            if (InventoryData.InventorySlots[i].ItemID == itemDefinition.ItemId && InventoryData.InventorySlots[i].ItemCount < itemDefinition.MaxStack)// If the item already exissts and there is space in the slot
            {
                int spaceAvailable = itemDefinition.MaxStack - InventoryData.InventorySlots[i].ItemCount;
                int toAdd = Math.Min(spaceAvailable, count);
                DefineItemToSlot(itemDefinition, toAdd, InventoryData.InventorySlots[i]);
                totalAdded += toAdd;
                count -= toAdd;

                if (count == 0) break;
            }
        }

        if (count > 0) // Try adding to empty slots if any items are still left to add
        {
            for (int i = 0; i < InventoryData.InventorySlots.Count && count > 0; i++)
            {
                bool containsAny = false;
                foreach (var gameplayTag in itemDefinition.ItemTags)
                {
                    if(InventoryData.InventorySlots[i].AllowedTags.HasTag(gameplayTag))
                    {
                        containsAny = true;
                        break;
                    }
                }
                if(!containsAny && InventoryData.InventorySlots[i].AllowedTags.TagCount > 0) continue;
                
                if (InventoryData.InventorySlots[i].ItemID.IsNullOrWhitespace())
                {
                    int spaceAvailable = itemDefinition.MaxStack;
                    if (itemDefinition.MaxStack == 0) spaceAvailable = 1;
                    int toAdd = Math.Min(spaceAvailable, count);
                    DefineItemToSlot(itemDefinition, toAdd, InventoryData.InventorySlots[i]);
                    totalAdded += toAdd;
                    count -= toAdd;
                }
            }
        }

        if (totalAdded == 0)
        {
            Debug.Log("Inventory is full or no suitable slot found.");
        }

        return totalAdded;
    }


    public bool ContainsItem(ItemDefinition itemDefinition, int count)
    {
        int availableCount = 0;
  
        foreach (var slot in InventoryData.InventorySlots)
        {
            if (slot.ItemID == itemDefinition.ItemId)
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
            if (slot.ItemID == itemDefinition.ItemId)
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
            if (InventoryData.InventorySlots[i].ItemID == itemDefinition.ItemId)
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
        Debug.Log("Adding " + count + " " + itemDefinition.ItemName + " to slot " + slot);
        slot.ItemCount += count;
        slot.ItemID = itemDefinition.ItemId;
        //slot.ItemData.Name = itemDefinition.ItemName;
        //TODO: security validation here
        SaveDataFirestore();
        onInventoryChanged?.Invoke();
    }
    [Serializable]
    public class ItemPack
    {
        public ItemDefinition ItemDefinition;
        public int Count;
    }


    #region Save/Load

    [Button]
    public void SaveInventory()
    {
        SaveDataFirestore();
    }

    public void SetInventoryData(InventoryData data)
    {
        InventoryData = data;
        onInventoryChanged?.Invoke();
    }
    
    public async void  SaveDataFirestore()
    {
        if(AuthManager.Instance.HasAnyConnection == false) return;
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
    }
    

    #endregion
    
}