using System.Collections;
using System.Collections.Generic;
using Core.Editor;
using StatSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class AbilityLevelNode : CodeFunctionNode
{
    [SerializeField] private float _value;
            public override float Value
            {
                get => _value;
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        OnValidateSelf();
                    }
                }
            }
    
            public override float CalculateValue(GameObject source)
            {
                AbilityController abilityController = source.GetComponentInChildren<AbilityController>();
                return abilityController.Abilities[AbilityName].level;
            }
    
            public string AbilityName;
            public Ability Ability;
    
            private void OnEnable()
            {
                RefreshValue();
            }
    
            private void OnDisable()
            {
                RefreshValue();
            }
    
            public void RefreshValue()
            {
                float tempVal = 0;
                if (Ability != null)
                {
                    tempVal = Ability.level;
                    DDebug.Log(AbilityName+"ability not null and ability level is " + Ability.level);
                }
    
                Value = tempVal;
                OnValidateSelf();
            }
           
    #if UNITY_EDITOR
            private void OnValidate()
            {
                OnValidateSelf();
            }
    #endif
}
