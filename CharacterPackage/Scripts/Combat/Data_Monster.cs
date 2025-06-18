using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data_Monster : Data
{
    [SerializeField]
    private MonsterDefinition _definition; 
    public MonsterDefinition Definition 
    {
        get => _definition;
        set => _definition = value;
    }
}
