using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Data_Living : Data
{
    [SerializeField]
    private bool _shouldDietrigger; 
    public bool ShouldDieTrigger 
    {
        get => _shouldDietrigger;
        set => _shouldDietrigger = value;
    }
}
