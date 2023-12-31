using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BindIntToText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tmPro;
    [SerializeField] private IntVar _intVar;

    [SerializeField] private string _prefix;
    [SerializeField] private int _preAddition;
    private void OnEnable()
    {
        _tmPro.text = _prefix+ (_intVar.Value + _preAddition);
        _intVar.OnValueChanged += OnVariableChanged;
    }

    private void OnDisable()
    {
        _intVar.OnValueChanged -= OnVariableChanged;
    }

    private void OnVariableChanged(int obj)
    {
        _tmPro.text = _prefix+ (obj + _preAddition);
    }
}
