using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemModifier", menuName = "Inventory System/Modifiers/Item Modifier Definition")]
public class ItemModifierDefinition : ScriptableObject
{
    public string ModifierID;
    public string Description;
    public ItemModifierValue[] ModifierValues;
    
    [Button]
    public void MatchIdWithName()
    {
        ModifierID = name.ToLower();
    }
}
[System.Serializable]
public class ItemModifierValue
{
    public int MinValue;
    public int MaxValue;
}

public class ItemModifierData
{
    public string ModifierID;
    public ItemModifierValue[] ModifierValues;
}