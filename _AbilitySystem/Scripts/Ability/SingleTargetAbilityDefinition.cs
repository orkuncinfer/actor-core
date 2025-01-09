using System;
using UnityEditor;
using UnityEngine;

[AbilityType(typeof(SingleTargetAbility))]
[CreateAssetMenu(fileName = "SingleTargetAbility", menuName = "AbilitySystem/Ability/SingleTargetAbility", order = 0)]
public class SingleTargetAbilityDefinition : ActiveAbilityDefinition
{
    [Space(20)]
    private GameplayEffectDefinition _cost;
    private GameplayPersistentEffectDefinition _cooldown;
    
}
