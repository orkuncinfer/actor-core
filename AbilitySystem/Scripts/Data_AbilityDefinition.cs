using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data_AbilityDefinition : Data
{
    [SerializeField]
    private AbilityDefinition _abilityDefinition; 
    public AbilityDefinition AbilityDefinition 
    {
        get => _abilityDefinition;
        set => _abilityDefinition = value;
    }
}
