using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Firestore;
using FishNet.Object;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

[CreateAssetMenu(fileName = "New Inventory Definition", menuName = "Inventory System/Inventory Definition")]
public class InventoryDefinition : MonoBehaviour
{
    public int InitialSlotCount;

    public GenericKey InventoryId;

    public bool CreateOnInitialize = false;

    public ItemPack[] InitialItems;

    public SaveMethods SaveMethod;

    public enum SaveMethods
    {
        ES3,
        Firebase,
        MySql,
        None
    }

    public InventoryData InventoryData = new InventoryData();

    private FirebaseDatabase _database;
    private FirebaseFirestore _firestore;

#if USING_FISHNET
    private NetworkObject _networkObject;
    private bool isServer => _networkObject.IsServerInitialized;
#endif

    public bool IsInitialized { get; set; }
    public Actor Owner { get; private set; }
    public event Action onInventoryChanged;
    public event Action onInventoryInitialized;

    private void Start()
    {
#if USING_FISHNET
        _networkObject = transform.parent.GetComponent<NetworkObject>();
#endif
        Initialize();

        if (SaveMethod == SaveMethods.ES3)
        {
            LoadES3();
        }

        onInventoryInitialized?.Invoke();
        IsInitialized = true;
    }

    private void Awake()
    {
        ActorUtilities.FindFirstActorInParents(transform).RegisterInventoryDefinition(this);
    }

    public void TriggerChanged()
    {
        onInventoryChanged?.Invoke();
    }

    [Button]
    public void CheckNulls()
    {
        foreach (var slot in InventoryData.InventorySlots)
        {
            if (slot.ItemData != null)
            {
                Debug.Log("Not null " + slot.ItemData.ItemID);
            }
        }
    }

    private void Initialize()
    {
        Owner = ActorUtilities.FindFirstActorInParents(transform);

        if (CreateOnInitialize)
        {
            InventoryData = new InventoryData();
            if (InventoryData.InventorySlots == null || InventoryData.InventorySlots.Count == 0)
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
            InventoryData.InventorySlots[i].ItemData = null;
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
                    Quantity = itemPack.Count
                };
                AddItem(itemPack.ItemDefinition, itemPack.Count, itemData);
            }
        }

        _database = FirebaseDatabase.GetInstance(
            "https://templateproject-174cf-default-rtdb.europe-west1.firebasedatabase.app/");
        _firestore = FirebaseFirestore.DefaultInstance;
    }

    [Button]
    public int AddItemWithDefinition(ItemDefinition itemDefinition, int count)
    {
        ItemData newItem = new ItemData { Quantity = count, ItemID = itemDefinition.ItemId };
        return AddItem(itemDefinition, count, newItem);
    }

    public int AddItem(ItemDefinition itemDefinition, int count, ItemData itemData)
    {
        if (!CheckEligible()) return 0;
        int totalAdded = 0;

        for (int i = 0; i < InventoryData.InventorySlots.Count; i++)
        {
            if (InventoryData.InventorySlots[i].ItemData == null) continue; // this slot is empty
            if (InventoryData.InventorySlots[i].ItemData.ItemID == itemData.ItemID &&
                InventoryData.InventorySlots[i].ItemData.Quantity < itemDefinition.MaxStack
                && itemDefinition
                    .IsStackable) // If the item already exists and there is space in the slot (increase its quantity only)
            {
                int spaceAvailable = itemDefinition.MaxStack - InventoryData.InventorySlots[i].ItemCount;
                int toAdd = Math.Min(spaceAvailable, count);
                InventoryData.InventorySlots[i].ItemData.Quantity += toAdd;
                totalAdded += toAdd;
                count -= toAdd;

                if (count == 0) break;
            }
        }

        Debug.Log($"Added {totalAdded} amount of  {itemData.ItemID} and still have {count} to add.");
        if (count > 0) // Try adding to empty slots if any items are still left to add
        {
            for (int i = 0; i < InventoryData.InventorySlots.Count && count > 0; i++)
            {
                if (!IsTagEligible(InventoryData.InventorySlots[i], itemDefinition))
                {
                    Debug.Log("Couldn't add item because no suitable tag slot " + itemDefinition.name);
                    continue;
                }

                if (InventoryData.InventorySlots[i].ItemData == null) // found empty slot
                {
                    int spaceAvailable = itemDefinition.MaxStack;
                    if (itemDefinition.MaxStack == 0 || !itemDefinition.IsStackable) spaceAvailable = 1;
                    int toAdd = Math.Min(spaceAvailable, count);
                    ItemData newItem = new ItemData { ItemID = itemData.ItemID, Quantity = toAdd };
                    InventoryData.InventorySlots[i].ItemData = newItem;
                    totalAdded += toAdd;
                    count -= toAdd;
                }
            }
        }

        if (totalAdded == 0)
        {
            Debug.Log("Inventory is full or no suitable slot found.");
        }
        else
        {
            onInventoryChanged?.Invoke();
        }

        return totalAdded;
    }

    private bool IsTagEligible(InventorySlot slot, ItemDefinition itemDefinition)
    {
        if (slot.AllowedTags.TagCount == 0) return true;
        return itemDefinition.ItemTags.HasAny(slot.AllowedTags);
    }

    private bool CheckEligible()
    {
        return true;
#if USING_FISHNET
        if (!isServer) return false;
#endif
        return true;
    }

    public bool
        ReplaceItem(ItemDefinition itemDefinition,
            ItemData itemData) // if it can equip entire slot anywhere without specifying a slot
    {
        if (!CheckEligible()) return false;
        for (int i = 0; i < InventoryData.InventorySlots.Count && itemData.Quantity > 0; i++)
        {
            if (!IsTagEligible(InventoryData.InventorySlots[i], itemDefinition))
            {
                Debug.Log("Couldn't add item because no suitable tag slot " + itemDefinition.name);
                continue;
            }

            DefineItemToSlot(itemDefinition, InventoryData.InventorySlots[i], itemData);
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
            if (InventoryData.InventorySlots[i].ItemID ==
                itemDefinition.ItemId) // If the item already exissts and there is space in the slot
            {
                slotIndex = i;
                return true;
            }
        }

        slotIndex = -1;
        return false;
    }

    public bool HaveSpace(ItemData itemData, out int slotIndex)
    {
        if (itemData == null) // have space for an empty item (no item)
        {
            slotIndex = -1;
            return true;
        }

        ItemDefinition itemDefinition = InventoryUtils.FindItemDefinitionWithId(itemData.ItemID);
        if (itemDefinition.IsStackable)
        {
            for (int i = 0; i < InventoryData.InventorySlots.Count; i++)
            {
                if (InventoryData.InventorySlots[i].ItemID == itemDefinition.ItemId &&
                    InventoryData.InventorySlots[i].ItemCount <
                    itemDefinition.MaxStack) // If the item already exissts and there is space in the slot
                {
                    slotIndex = i;
                    return true;
                }
            }
        }

        for (int i = 0;
             i < InventoryData.InventorySlots.Count && itemData.Quantity > 0;
             i++) // search for empty and allowed tagged slot.
        {
            if (!IsTagEligible(InventoryData.InventorySlots[i], itemDefinition)) continue;

            if (InventoryData.InventorySlots[i].ItemData == null) // have empty slot
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
            if(slot.ItemData == null) continue;
            if (slot.ItemID == itemDefinition.ItemId)
            {
                availableCount += slot.ItemCount;
            }
        }

        return availableCount;
    }

    public void RemoveItem(ItemDefinition itemDefinition, int count)
    {
        if (!CheckEligible()) return;
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
                    InventoryData.InventorySlots[i].ItemData.Quantity -= count;
                    break; // Completed the removal requirement
                }
                else
                {
                    count -= InventoryData.InventorySlots[i].ItemCount;
                    InventoryData.InventorySlots[i].ItemData.Quantity = 0;
                    RemoveEntireSlot(InventoryData.InventorySlots[i]);
                    //ResetItemData(InventoryData.InventorySlots[i].ItemData); // Reset the data as this slot is now empty*******????????
                }
            }
        }
    }

    public void RemoveEntireSlot(InventorySlot slot)
    {
        if (!CheckEligible()) return;
        slot.ItemData = null;
        onInventoryChanged?.Invoke();
    }

    private bool ValidateSwapOrMerge(int index1, int index2, out InventorySlot fromSlot, out InventorySlot targetSlot,
        out ItemDefinition itemDefinition1, out ItemDefinition itemDefinition2,
        InventoryDefinition targetInventory = null)
    {
        if (!CheckEligible() || index1 == index2)
        {
            fromSlot = null;
            targetSlot = null;
            itemDefinition1 = null;
            itemDefinition2 = null;
            return false;
        }

        fromSlot = InventoryData.InventorySlots[index1];

        if (targetInventory != null)
        {
            Debug.Log("Target inventory " + targetInventory.transform.name + "i:" + index2);
            targetSlot = targetInventory.InventoryData.InventorySlots[index2];
        }
        else
        {
            targetSlot = InventoryData.InventorySlots[index2];
        }

        if (fromSlot.ItemData == null)
        {
            itemDefinition1 = null;
            itemDefinition2 = null;
            return false;
        }

        itemDefinition1 = InventoryUtils.FindItemDefinitionWithId(fromSlot.ItemID);

        if (itemDefinition1 == null)
        {
            itemDefinition2 = null;
            return false;
        }

        if (targetSlot.ItemData != null)
        {
            itemDefinition2 = InventoryUtils.FindItemDefinitionWithId(targetSlot.ItemID);

            // Check for merge case
            if (fromSlot.ItemID == targetSlot.ItemID)
            {
                return true;
            }

            // Check tag eligibility for both slots
            if (!IsTagEligible(fromSlot, itemDefinition2))
            {
                return false;
            }
        }
        else
        {
            itemDefinition2 = null;
        }

        if (!IsTagEligible(targetSlot, itemDefinition1))
        {
            return false;
        }

        return true;
    }

    public bool CanSwapOrMergeItems(int index1, int index2, InventoryDefinition targetInventory = null)
    {
        return ValidateSwapOrMerge(index1, index2, out _, out _, out _, out _,targetInventory);
    }

    public void SwapOrMergeItems(int index1, int index2, InventoryDefinition targetInventory = null)
    {
        if (!ValidateSwapOrMerge(index1, index2, out InventorySlot fromSlot, out InventorySlot targetSlot,
                out ItemDefinition itemDefinition1, out ItemDefinition itemDefinition2, targetInventory))
        {
            return;
        }

        // If both slots have the same item type, try to merge
        if (targetSlot.ItemData != null && fromSlot.ItemID == targetSlot.ItemID)
        {
            MergeItems(index1, index2);
        }
        else
        {
            // Otherwise perform a swap
            SwapItems(fromSlot, targetSlot);
        }
    }

    public void MergeItems(int index1, int index2)
    {
        InventorySlot fromSlot = InventoryData.InventorySlots[index1];
        InventorySlot targetSlot = InventoryData.InventorySlots[index2];

        if (fromSlot.ItemID != targetSlot.ItemID)
        {
            return;
        }

        ItemDefinition itemDefinition = InventoryUtils.FindItemDefinitionWithId(fromSlot.ItemID);

        if (!itemDefinition.IsStackable)
        {
            return;
        }

        int emptyCount = itemDefinition.MaxStack - targetSlot.ItemCount;

        if (emptyCount <= 0)
        {
            return;
        }

        if (emptyCount >= fromSlot.ItemCount)
        {
            targetSlot.ItemData.Quantity += fromSlot.ItemCount;
            RemoveEntireSlot(fromSlot);
        }
        else
        {
            fromSlot.ItemData.Quantity -= emptyCount;
            targetSlot.ItemData.Quantity += emptyCount;
        }

        onInventoryChanged?.Invoke();
    }

    public void SwapItems(InventorySlot slot1, InventorySlot slot2)
    {
        if (!CheckEligible() || slot1 == slot2)
        {
            return;
        }

        (slot1.ItemData, slot2.ItemData) = (slot2.ItemData, slot1.ItemData);
        onInventoryChanged?.Invoke();
    }

    private void DefineItemToSlot(ItemDefinition itemDefinition, InventorySlot slot, ItemData itemData)
    {
        Debug.Log("Adding " + itemData.Quantity + " " + itemDefinition.ItemName + " to slot " + slot);

        if (itemData != null)
        {
            slot.ItemData = itemData;
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

    public async void SaveDataFirestore()
    {
        if (AuthManager.Instance.HasAnyConnection == false) return;
        //string json = JsonConvert.SerializeObject(InventoryData);
        //Debug.Log("data is " + json);
        var playerData = new Dictionary<string, object>
        {
            { InventoryId.ID, InventoryData },
        };

        await _firestore.Collection("player-data").Document(FirebaseAuth.DefaultInstance.CurrentUser.UserId)
            .Collection("Inventories").Document(InventoryId.ID).SetAsync(InventoryData).ContinueWithOnMainThread(task =>
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
        if (SaveMethod == SaveMethods.ES3)
        {
            SaveES3();
        }
    }

    public void SaveES3()
    {
        string saveFileName = "Inventory_" + InventoryId.ID + ".save";
        ES3.Save(InventoryId.ID, InventoryData, saveFileName);
    }

    public void LoadES3()
    {
        string saveFileName = "Inventory_" + InventoryId.ID + ".save";
        if (ES3.KeyExists(InventoryId.ID, saveFileName))
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