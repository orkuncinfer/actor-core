using System;
using System.Linq;
using Sirenix.OdinInspector;
using StatSystem;
using UnityEngine;

public class EffectApplierTest : MonoBehaviour
{
    public GameObject Target;

    public GameplayEffectDefinition ApplyEffect;

    [Button]
    public void Apply()
    {
     
        GameplayEffectController effectController = Target.GetComponentInChildren<GameplayEffectController>();
        //effectController.ApplyGameplayEffectDefinition(ApplyEffect.ItemId);
        
        EffectTypeAttribute attribute = ApplyEffect.GetType().GetCustomAttributes(true)
            .OfType<EffectTypeAttribute>().FirstOrDefault();
        GameplayEffect effect =
            Activator.CreateInstance(attribute.type, ApplyEffect, this, effectController.gameObject) as
                GameplayEffect;

        if (effect is GameplayPersistentEffect pers)
        {
            //effectController.ApplyGameplayEffectToSelf(pers);
        }
    }
    [Button]
    public void AddModifiers()
    {
        StatController statController = Target.GetComponentInChildren<StatController>();

        StatModifier modifierToAdd = new StatModifier();
        modifierToAdd.Magnitude = 10;
        modifierToAdd.Type = ModifierOperationType.Additive;
        modifierToAdd.Source = this;

        statController.GetStat("AttackPower").AddModifier(modifierToAdd);
    }
}
