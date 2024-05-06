using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "UniqItem_", menuName = "Inventory System/Items/Unique Item Definition")]
public class UniqueItemDefinition : ItemDefinition
{
    public ItemModifierDefinition[] PossibleModifiers;
}
