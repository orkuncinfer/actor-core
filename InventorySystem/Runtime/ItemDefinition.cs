using System;
using System.Collections;
using Firebase.Firestore;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
[SOCreatable]
[CreateAssetMenu(fileName = "New Item Definition", menuName = "Inventory System/Items/Item Definition")]
public class ItemDefinition : ItemBaseDefinition
{
    public GameObject WorldPrefab;
    public GameObject UIPrefab;
    [FormerlySerializedAs("ItemType")] public RpgItemTypes _rpgItemTypes;
    public bool IsStackable;
    public int MaxStack;
    public ItemRarity DefaultRarity;
    public bool IsUniqueItem;
    
}

[FirestoreData(ConverterType = typeof(FirestoreEnumNameConverter<RpgItemTypes>))]
public enum RpgItemTypes
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

public enum ItemTypes
{
    Default,
    Consumable,
    Upgrade,
    Ability,
    Mission,
}
