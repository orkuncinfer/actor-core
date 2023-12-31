using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "IntVar_", menuName = "Variables/IntVar", order = 1)]
public class IntVar : ScriptableObject
{
    
    public event Action<int> OnValueChanged;
    private int _value;
    public int Value
    {
        get { return _value; }
        set
        {
            if (_value != value)
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }
    }
    
    public void Reset()
    {
        Value = 0;
    }

    private void OnDisable()
    {
        Reset();
    }

    private void OnDestroy()
    {
        Value = 0;
    }
}
