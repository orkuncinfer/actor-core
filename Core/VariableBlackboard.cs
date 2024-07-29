using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using WolarGames.Variables;

public class VariableBlackboard : MonoBehaviour
{
    [SerializeField] private List<BlackboardVariable> Variables = new List<BlackboardVariable>();

    private void Start()
    {
        LoadAllVariables();
    }

    private void OnDestroy()
    {
        SaveAllVariables();
    }

    private void SaveAllVariables()
    {
        foreach (var variable in Variables)
        {
            variable.Asset.SaveVariable();
        }
    }
    
    private void LoadAllVariables()
    {
        foreach (var variable in Variables)
        {
            variable.Asset.LoadVariable();
        }
    }
}
[Serializable]
public class BlackboardVariable
{
    [HorizontalGroup]public Variable<object> Asset;
    [HorizontalGroup]public object Value;
}
