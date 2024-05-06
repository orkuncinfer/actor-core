using System;
using System.Collections;
using Firebase.Firestore;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Definition", menuName = "Inventory System/Items/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public string ID;
    public string ItemName;
    public string Description;
    public Sprite Icon;
    public GameObject Prefab;
    public ItemType ItemType;
    public bool IsStackable;
    public int MaxStack;
    public ItemRarity DefaultRarity;
    public bool IsUniqueItem;

}

[FirestoreData(ConverterType = typeof(FirestoreEnumNameConverter<ItemType>))]
public enum ItemType
{
    Default,
    Consumable,
    Helmet,
    Weapon,
    Shield,
    Boots,
    Chest,
}
[FirestoreData(ConverterType = typeof(FirestoreEnumNameConverter<ItemRarity>))]
public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum Attributes
{
    Agility,
    Intellect,
    Stamina,
    Strength
}
