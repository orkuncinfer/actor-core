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
    public GameObject Model;
    public GameObject DropPrefab;
    public GameObject UIPrefab;
    [FormerlySerializedAs("ItemType")] public RpgItemTypes _rpgItemTypes;
    public bool IsStackable;
    public int MaxStack;
    public ItemRarity DefaultRarity;
    public bool IsUniqueItem;

    public GameplayTagContainer ItemTags;

    
    [SerializeReference][ValueDropdown("GetFilteredActionList")] [ListDrawerSettings(ShowFoldout = true)]
    public List<SimpleAction> ItemActions = new List<SimpleAction>();



#if UNITY_EDITOR
    public List<ValueDropdownItem<Type>> GetFilteredActionList()
    {
        var baseType = typeof(SimpleAction);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var q = assemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsGenericTypeDefinition)
            .Where(x => baseType.IsAssignableFrom(x) && x != baseType); // Exclude the base class itself

        List<ValueDropdownItem<Type>> returnList = new List<ValueDropdownItem<Type>>();
        
        foreach (var type in q)
        {
            returnList.Add(new ValueDropdownItem<Type>(type.Name, type));
        }
        
        return returnList;
    }
#endif
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
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4
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
