using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class DataVar_Float : Data
{
    [SerializeField][HideInPlayMode]
    private float _value;
    public event Action<float,float> onValueChanged;
    [ShowInInspector][HideInEditorMode]
    public float Value
    {
        get => _value;
        set
        {
            float oldValue = _value;
            bool isChanged = _value != value;
            _value = value;
            if (isChanged)
            {
                onValueChanged?.Invoke(oldValue,value);
            }
        }
    }
}