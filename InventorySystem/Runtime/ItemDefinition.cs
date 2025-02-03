using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
[SOCreatable("Items")]
public class ItemDefinition : ItemBaseDefinition
{
    public GameObject WorldPrefab;
    public GameObject UIPrefab;
    [FormerlySerializedAs("ItemType")] public RpgItemTypes _rpgItemTypes;
    public bool IsStackable;
    public int MaxStack;
    public ItemRarity DefaultRarity;
    public bool IsUniqueItem;

    public GameplayTagContainer ItemTags;

    
    [SerializeReference][TypeFilter("GetFilteredActionList")] [ListDrawerSettings(ShowFoldout = true)]
    public List<SimpleAction> ItemActions = new List<SimpleAction>();
    
  
    
    
    public IEnumerable<Type> GetFilteredActionList()
    {
        var baseType = typeof(SimpleAction);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var q = assemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsGenericTypeDefinition)
            .Where(x => baseType.IsAssignableFrom(x) && x != baseType); // Exclude the base class itself
        return q;
    }
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
