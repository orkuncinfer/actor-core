using System.Collections;
using System.Collections.Generic;
using Core.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class FormulaField
{
    [HorizontalGroup]
    public NodeGraph Graph;
    [HorizontalGroup]
    public float StaticValue;

    public float CalculateValue(GameObject instigator = null)
    {
        float value = 0;
        if (Graph && instigator != null)
        {
            value = Graph.CalculateValue(instigator);
        }
        else
        {
            value = StaticValue;
        }

        return value;
    }
}