using UnityEngine;

/// <summary>
/// Represents an item dropped in the game world
/// </summary>
public class ItemDrop : MonoBehaviour
{
    [System.Serializable]
    public enum Rarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4
    }
    
    [Header("Item Properties")]
    [Tooltip("The name of the item")]
    [SerializeField] private string _itemName = "Item";
    
    [Tooltip("The rarity of the item")]
    [SerializeField] private Rarity _itemRarity = Rarity.Common;
    
    [Tooltip("The quantity of the item")]
    [SerializeField] private int _quantity = 1;
    
    [Header("Behavior")]
    [Tooltip("Should this item automatically register with the ItemDropManager?")]
    [SerializeField] private bool _autoRegister = true;
    
    
    // Reference to the drop manager
    private LootLabelSystem.ItemDropManager _dropManager;
    
    private void Start()
    {
        if (_autoRegister)
        {
            // Find the item drop manager
            _dropManager = Object.FindFirstObjectByType<LootLabelSystem.ItemDropManager>();
            
            if (_dropManager != null)
            {
                // Register with the manager
                _dropManager.RegisterItemDrop(this);
            }
            else
            {
                Debug.LogWarning("ItemDropManager not found in scene. Item label will not be created.");
            }
        }
    }
    
    private void OnDestroy()
    {
        // Unregister from the manager when destroyed
        if (_dropManager != null)
        {
            _dropManager.UnregisterItemDrop(this);
        }
    }
    
    /// <summary>
    /// Gets the display name for this item
    /// </summary>
    public string GetItemName()
    {
        if (_quantity > 1)
        {
            return $"{_itemName} x{_quantity}";
        }
        
        return _itemName;
    }
    
    /// <summary>
    /// Gets the rarity level as an integer for priority sorting
    /// </summary>
    public int GetItemRarity()
    {
        return (int)_itemRarity;
    }
    
    /// <summary>
    /// Sets the item's name
    /// </summary>
    public void SetItemName(string name)
    {
        _itemName = name;
    }
    
    /// <summary>
    /// Sets the item's rarity
    /// </summary>
    public void SetItemRarity(Rarity rarity)
    {
        _itemRarity = rarity;
    }
    
    /// <summary>
    /// Sets the item's quantity
    /// </summary>
    public void SetQuantity(int quantity)
    {
        _quantity = Mathf.Max(1, quantity);
    }
    
    /// <summary>
    /// Register this item with a specific drop manager
    /// </summary>
    public void RegisterWithManager(LootLabelSystem.ItemDropManager manager)
    {
        if (manager != null)
        {
            _dropManager = manager;
            _dropManager.RegisterItemDrop(this);
        }
    }
}