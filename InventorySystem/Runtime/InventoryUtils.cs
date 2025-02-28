using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public static class InventoryUtils
{
    private static Dictionary<string, ItemDefinition> itemDictionary;
    private static bool isInitialized = false;

    private static Texture2D _defaultBadge;
    private static Texture2D _legendaryBadge;

    private static Sprite _defaultBadgeSprite;
    private static Sprite _legendaryBadgeSprite;

    private static ItemDataBaseDefinition _itemDataBase;

    public static Sprite GetRarityBadge(ItemRarity rarity)
    {
        if (_defaultBadge == null)
            _defaultBadge = Resources.Load<Texture2D>("DefaultBadge");
        if (_legendaryBadge == null)
            _legendaryBadge = Resources.Load<Texture2D>("LegendaryBadge");

        switch (rarity)
        {
            case ItemRarity.Common:
                if (_defaultBadgeSprite == null)
                {
                    _defaultBadgeSprite = Texture2DToSprite(_defaultBadge);
                }
                return _defaultBadgeSprite;
            case ItemRarity.Legendary:
                if (_legendaryBadgeSprite == null)
                {
                    _legendaryBadgeSprite = Texture2DToSprite(_legendaryBadge);
                }
                return _legendaryBadgeSprite;
            default:
                return Texture2DToSprite(_defaultBadge);
        }
    }

    private static Sprite Texture2DToSprite(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("Texture2D to Sprite conversion failed: Texture2D is null");
            return null;
        }
        Debug.Log("Created new sprite from texture. !! this is costly operation !!");
        return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    public static ItemDefinition FindItemWithId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("Item id is null or empty");
            return null;
        }
        EnsureInitialized();
        if (itemDictionary.TryGetValue(id, out ItemDefinition item))
        {
            return item;
        }
        else
        {
            Debug.LogWarning($"Item with ID {id} not found.");
            return null;
        }
    }

    private static void EnsureInitialized()
    {
        if (!isInitialized)
        {
            InitializeDictionary();
            isInitialized = true;
        }
    }

    private static void InitializeDictionary()
    {
        _itemDataBase = Resources.Load<ItemDataBaseDefinition>("ItemDataBase");
        itemDictionary = new Dictionary<string, ItemDefinition>();

        foreach (ItemDefinition item in _itemDataBase.AllItems)
        {
            if (!itemDictionary.ContainsKey(item.ItemId))
            {
                itemDictionary.Add(item.ItemId, item);
            }
            else
            {
                Debug.LogWarning($"Duplicate ID {item.ItemId} found in ItemDataBase. Skipping this item.");
            }
        }
    }
}