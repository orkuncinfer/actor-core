using UnityEngine;

[AbilityType(typeof(ActiveAbility))]
[CreateAssetMenu(fileName = "NonTargetedAbility", menuName = "AbilitySystem/Ability/NonTargetedAbility", order = 0)]
public class NonTargetedAbilityDefinition : ActiveAbilityDefinition
{
    [Space(20)]
    private GameplayEffectDefinition _cost;
    private GameplayPersistentEffectDefinition _cooldown;
}