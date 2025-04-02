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
public class InventoryDefinition : MonoBehaviour        
{

    public int InitialSlotCount;
    public int Width;
    public GenericKey InventoryId;

    public bool CreateOnInitialize = false;
    
    public InventoryData InventoryData = new InventoryData();
    
    public ItemPack[] InitialItems;

   public event Action onInventoryChanged;

   public event Action onInventoryInitialized;

    private string savePath => "/" + InventoryId.ID;
    private FirebaseDatabase _database;
    private FirebaseFirestore _firestore;

    private void Start()
    {
        Initialize();
        
        LoadES3();
        
        onInventoryInitialized?.Invoke();
        Debug.Log("Inventory count is " + DefaultPlayerInventory.Instance.InventoryDefinitions.Count);
    }

    private void Awake()
    {
        Debug.Log($"Registering inventory and count is {DefaultPlayerInventory.Instance.InventoryDefinitions.Count}");
        DefaultPlayerInventory.Instance.RegisterInventoryDefinition(this);
        Debug.Log($"Registered Inventory and count is {DefaultPlayerInventory.Instance.InventoryDefinitions.Count}");
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

        for (int i = 0; i < InventoryData.InventorySlots.Count; i++)
        {
            InventoryData.InventorySlots[i].SlotIndex = i;
        }
        foreach (var itemPack in InitialItems)
        {
            Debug.Log("tried to add " + itemPack.ItemDefinition.ItemName + " to inventory");
            if (itemPack.ItemDefinition != null)
            {
                Debug.Log("added item " + itemPack.ItemDefinition.ItemName + " to inventory");
                ItemData itemData = new ItemData
                {
                   ItemID = itemPack.ItemDefinition.ItemID,
                };
                AddItem(itemPack.ItemDefinition, itemPack.Count,itemData);
            }
        }
        _database = FirebaseDatabase.GetInstance("https://templateproject-174cf-default-rtdb.europe-west1.firebasedatabase.app/");
        _firestore = FirebaseFirestore.DefaultInstance;
    }
    [Button]
    public int AddItem(ItemDefinition itemDefinition, int count, ItemData itemData = null)
    {
        int totalAdded = 0;

        for (int i = 0; i < InventoryData.InventorySlots.Count; i++)
        {
            if (InventoryData.InventorySlots[i].ItemID == itemDefinition.ItemId && InventoryData.InventorySlots[i].ItemCount < itemDefinition.MaxStack
                && itemDefinition.IsStackable)// If the item already exissts and there is space in the slot
            {
                int spaceAvailable = itemDefinition.MaxStack - InventoryData.InventorySlots[i].ItemCount;
                int toAdd = Math.Min(spaceAvailable, count);
                AddItemToSlot(itemDefinition, toAdd, InventoryData.InventorySlots[i],itemData);
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
                foreach (var gameplayTag in itemDefinition.ItemTags.GetTags())
                {
                    if(InventoryData.InventorySlots[i].AllowedTags.HasTag(gameplayTag))
                    {
                        containsAny = true;
                        break;
                    }
                }
                
                if (!containsAny && InventoryData.InventorySlots[i].AllowedTags.TagCount > 0)
                {
                    Debug.Log("Couldn't add item because no suitable tag slot " + itemDefinition.name);
                    continue;
                }
                
                if (InventoryData.InventorySlots[i].ItemID.IsNullOrWhitespace())
                {
                    int spaceAvailable = itemDefinition.MaxStack;
                    if (itemDefinition.MaxStack == 0 || !itemDefinition.IsStackable) spaceAvailable = 1;
                    int toAdd = Math.Min(spaceAvailable, count);
                    AddItemToSlot(itemDefinition, toAdd, InventoryData.InventorySlots[i],itemData);
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

    public bool ReplaceItem(ItemDefinition itemDefinition,int count,ItemData itemData)
    {
        for (int i = 0; i < InventoryData.InventorySlots.Count && count > 0; i++)
        {
            bool containsAny = false;
            foreach (var gameplayTag in itemDefinition.ItemTags.GetTags())
            {
                if(InventoryData.InventorySlots[i].AllowedTags.HasTag(gameplayTag))
                {
                    containsAny = true;
                    break;
                }
            }
                
            if (!containsAny && InventoryData.InventorySlots[i].AllowedTags.TagCount > 0)
            {
                Debug.Log("Couldn't add item because no suitable tag slot " + itemDefinition.name);
                continue;
            }
            DefineItemToSlot(itemDefinition,count, InventoryData.InventorySlots[i],itemData);
            return true;
        }
        return false;
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

    public bool ContainsItemAny(ItemDefinition itemDefinition, out int slotIndex)
    {
        for (int i = 0; i < InventoryData.InventorySlots.Count; i++)
        {
            if (InventoryData.InventorySlots[i].ItemID == itemDefinition.ItemId)// If the item already exissts and there is space in the slot
            {
                slotIndex = i;
                return true;
            }
        }

        slotIndex = -1;
        return false;
    }

    public bool HaveSpace(ItemDefinition itemDefinition, int count, out int slotIndex)
    {
        if (itemDefinition.IsStackable)
        {
            for (int i = 0; i < InventoryData.InventorySlots.Count; i++)
            {
                if (InventoryData.InventorySlots[i].ItemID == itemDefinition.ItemId && InventoryData.InventorySlots[i].ItemCount < itemDefinition.MaxStack)// If the item already exissts and there is space in the slot
                {
                    slotIndex = i;
                    return true;
                }
            }
        }
        
        for (int i = 0; i < InventoryData.InventorySlots.Count && count > 0; i++)// search for empty and allowed tagged slot.
        {
            bool containsAny = false;
            foreach (var gameplayTag in itemDefinition.ItemTags.GetTags())
            {
                if(InventoryData.InventorySlots[i].AllowedTags.HasTag(gameplayTag))
                {
                    containsAny = true;
                    break;
                }
            }
            if(!containsAny && InventoryData.InventorySlots[i].AllowedTags.TagCount > 0) continue;
                
            if (InventoryData.InventorySlots[i].ItemID.IsNullOrWhitespace()) // have empty slot
            {
                slotIndex = i;
                return true;
            }
        }

        slotIndex = -1;
        return false;
    }
    public int GetItemCount(ItemDefinition itemDefinition)
    {
        if (itemDefinition == null)
        {
            Debug.LogError("itemdefinition is null");
            return default;
        }
        int availableCount = 0;
        foreach (var slot in InventoryData.InventorySlots)
        {
            if (slot.ItemID == itemDefinition.ItemId)
            {
                availableCount += slot.ItemCount;
            }
        }
        return availableCount;
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
        onInventoryChanged?.Invoke();
    }
    public void SwapOrMergeItems(int index1, int index2)
    {
        if (InventoryData.InventorySlots[index1].ItemID == InventoryData.InventorySlots[index2].ItemID) // merge
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
        onInventoryChanged?.Invoke();
    }
    private void AddItemToSlot(ItemDefinition itemDefinition, int count, InventorySlot slot, ItemData itemData = null)
    {
        Debug.Log("Adding " + count + " " + itemDefinition.ItemName + " to slot " + slot);
        slot.ItemCount += count;
        slot.ItemID = itemDefinition.ItemId;
        if (itemData != null) slot.ItemData = itemData;
        //slot.ItemData.Name = itemDefinition.ItemName;
        //TODO: security validation here
        SaveDataFirestore();
        onInventoryChanged?.Invoke();
    }
    private void DefineItemToSlot(ItemDefinition itemDefinition, int count, InventorySlot slot, ItemData itemData = null)
    {
        Debug.Log("Adding " + count + " " + itemDefinition.ItemName + " to slot " + slot);
        slot.ItemCount = count;
        
        if (itemData != null)
        {
            slot.ItemData = itemData;
            Debug.Log("setitemdata");
            slot.ItemID = itemDefinition.ItemId;
        }
        else
        {
            slot.ItemData = null;
        }
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
            {InventoryId.ID, InventoryData},
        };
        
        await _firestore.Collection("player-data").Document(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Collection("Inventories").Document(InventoryId.ID).SetAsync(InventoryData).ContinueWithOnMainThread(task =>
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

    private void OnDestroy()
    {
        SaveES3();
    }

    public void SaveES3()
    {
        string saveFileName ="Inventory_" + InventoryId.ID + ".save";
        ES3.Save(InventoryId.ID, InventoryData,saveFileName);
    }
    public void LoadES3()
    {
        string saveFileName ="Inventory_" + InventoryId.ID + ".save";
        if (ES3.KeyExists(InventoryId.ID,saveFileName))
        {
            InventoryData = ES3.Load(InventoryId.ID, saveFileName, InventoryData);
            onInventoryChanged?.Invoke();
        }
    }
    public virtual void MergePersistentData(Data loadedData)
    {
        // Implement type-specific merging in child classes
        if (loadedData.GetType() != GetType())
        {
            Debug.LogError("Data type mismatch during merge");
            return;
        }
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(loadedData), this);
    }
    #endregion
    
}