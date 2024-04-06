using StatSystem;
using UnityEngine;
[System.Serializable]
public class GameplayEffectDamageDefinition : AbstractGameplayEffectStatModifier
{
    public GameplayEffectDamageDefinition()
    {
        StatName = "Health";
        Type = ModifierOperationType.Additive;
    }
    public bool CanCriticalHit;
}
