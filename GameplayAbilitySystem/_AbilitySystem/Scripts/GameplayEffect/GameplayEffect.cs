using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using StatSystem;
using UnityEngine;

public class GameplayEffect
{
    protected GameplayEffectDefinition _definition;
    public GameplayEffectDefinition Definition => _definition;
    
    private object _source;
    public object Source
    {
        get => _source;
        set => _source = value;
    }

    private GameObject _instigator;
    public GameObject Instigator => _instigator; // who initiated the action

    protected StatController _statController;

    private List<StatModifier> _modifiers = new List<StatModifier>();
    public ReadOnlyCollection<StatModifier> Modifiers => _modifiers.AsReadOnly();

    public GameplayEffect(GameplayEffectDefinition definition, object source, GameObject instigator,GameObject victim)
    {
        _definition = definition;
        _source = source;
        _instigator = instigator;
        
        _statController = instigator.GetComponentInChildren<StatController>();
        
        foreach (AbstractGameplayEffectStatModifier modifierDef in definition.ModifierDefinitions)
        {
            StatModifier statModifier = CreateModifier(modifierDef, instigator);
            statModifier.Victim = victim;
            statModifier.Source = source;
            statModifier.Instigator = instigator;
            statModifier.Type = modifierDef.Type;
            statModifier.ModifierDefinition = modifierDef;
            _modifiers.Add(statModifier);
        }
    }

    private StatModifier CreateModifier(AbstractGameplayEffectStatModifier modifierDef, GameObject instigator)
    {
        if (modifierDef is GameplayEffectDamageDefinition damageDefinition)
        {
            return CreateHealthModifier(damageDefinition, instigator);
        }

        float magnitude = CalculateMagnitude(modifierDef, instigator);
        return new StatModifier
        {
            Magnitude = magnitude,
        };
    }

    private HealthModifier CreateHealthModifier(GameplayEffectDamageDefinition damageDefinition, GameObject instigator)
    {
        StatController statController = _statController;
        float calculatedMagnitude = damageDefinition.Formula.CalculateValue(instigator);

        HealthModifier healthModifier = new HealthModifier
        {
            Magnitude = calculatedMagnitude,
            IsCriticalHit = false,
        };

        if (damageDefinition.CanCriticalHit && statController != null)
        {
            float critChance = statController.Stats["CritChance"].Value / 100f;
            if (critChance >= Random.value)
            {
                float critMultiplier = 1 + (statController.Stats["CritMultiplier"].Value / 100f);
                healthModifier.Magnitude *= critMultiplier;
                healthModifier.IsCriticalHit = true;
            }
        }

        return healthModifier;
    }

    protected float CalculateMagnitude(AbstractGameplayEffectStatModifier modifierDef, GameObject instigator)
    {
        if (modifierDef.Formula.Graph != null)
        {
            return modifierDef.Formula.Graph.CalculateValue(instigator);
        }
        return modifierDef.Formula.StaticValue;
    }

    public override string ToString()
    {
        return ReplaceMacro(Definition.Description, this);
    }

    protected string ReplaceMacro(string value, object @object)
    {
        return Regex.Replace(value, @"{(.+?)}", match =>
        {
            var p = Expression.Parameter(@object.GetType(), @object.GetType().Name);
            var e = System.Linq.Dynamic.Core.DynamicExpressionParser.ParseLambda(new[] { p }, null,
                match.Groups[1].Value);
            return (e.Compile().DynamicInvoke(@object) ?? "").ToString();
        });
    }
}
