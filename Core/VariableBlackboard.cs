using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

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
            variable.Variable.SaveVariable();
        }
    }
    
    private void LoadAllVariables()
    {
        foreach (var variable in Variables)
        {
            variable.Variable.LoadVariable();
        }
    }
}
[Serializable]
public class BlackboardVariable
{
    [HorizontalGroup]public ScriptableVar Variable;
}
