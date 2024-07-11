using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class DataVar_Int : Data
{
    [SerializeField][HideInPlayMode]
    private int _value;
    public event Action<int,int> onValueChanged;
    [ShowInInspector][HideInEditorMode]
    public int Value
    {
        get => _value;
        set
        {
            int oldValue = _value;
            bool isChanged = _value != value;
            _value = value;
            if (isChanged)
            {
                onValueChanged?.Invoke(oldValue,value);
            }
        }
    }
}