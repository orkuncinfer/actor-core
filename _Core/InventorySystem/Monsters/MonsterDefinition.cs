using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
[CreateAssetMenu(fileName = "Monster_", menuName = "Inventory System/Monster/Monster Definition")]
public class MonsterDefinition : ItemBaseDefinition
{
    public string MonsterID;
    public ItemDrop[] PossibleDrops;
    
    public int MinGoldDrop;
    public int MaxGoldDrop;
}
[System.Serializable]
public class ItemDrop
{
    public ItemDefinition Item;
    public string ItemID;
    [Range(0, 1)] public float DropChance;
}
