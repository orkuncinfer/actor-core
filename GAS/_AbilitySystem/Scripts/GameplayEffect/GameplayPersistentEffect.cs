using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameplayPersistentEffect : GameplayEffect
{
    public new GameplayPersistentEffectDefinition Definition => _definition as GameplayPersistentEffectDefinition;
    public float RemainingDuration;
    
    public float RemainingPeriod;

    [ShowInInspector] private string _name => Definition.name;
    
    private float _duration;
    public float Duration => _duration;
    
    public GameplayPersistentEffect(GameplayPersistentEffectDefinition definition, object source, GameObject instigator) : base(definition, source, instigator)
    {
        RemainingPeriod = definition.Period;
        if (!definition.IsInfinite)
        {
            RemainingDuration = _duration = definition.DurationFormula.CalculateValue(instigator);
        }
    }
}
