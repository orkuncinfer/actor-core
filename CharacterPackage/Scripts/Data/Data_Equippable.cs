using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Data_Equippable : Data
{
    public string EquipSlotName => _equipSlotName;
    [SerializeField]private string _equipSlotName;
    
    public string UnequipSlotName => _unequipSlotName;
    [SerializeField]private string _unequipSlotName;
}
