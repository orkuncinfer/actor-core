using System.Collections;
using System.Collections.Generic;
using Core.Editor;
using StatSystem;
using UnityEngine;
[System.Serializable]
public  class AbstractGameplayEffectStatModifier
{
    public string StatName;
    public ModifierOperationType Type;
    public FormulaField Formula;
}