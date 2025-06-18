using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class Data_RefVar : Data
{
    [SerializeField][HideInPlayMode]
    private Object _value;
    public event Action<Object,Object> onValueChanged;
    [ShowInInspector][HideInEditorMode]
    public Object Value
    {
        get => _value;
        set
        {
            Object oldValue = _value;
            bool isChanged = _value != value;
            _value = value;
            if (isChanged)
            {
                onValueChanged?.Invoke(oldValue,value);
            }
        }
    }
}
