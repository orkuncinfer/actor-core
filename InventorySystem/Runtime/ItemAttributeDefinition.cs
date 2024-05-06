using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Attribute Definition", menuName = "Inventory System/Items/Item Attribute Definition")]
public class ItemAttributeDefinition : ScriptableObject
{
    public string AttributeID;
    public string Description;
    public ItemAttribute Attribute;
}
