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
    public object Source => _source;

    private GameObject _instigator;
    public GameObject Instigator => _instigator;

    private List<StatModifier> _modifiers = new List<StatModifier>();
    public ReadOnlyCollection<StatModifier> Modifiers => _modifiers.AsReadOnly();

    public GameplayEffect(GameplayEffectDefinition definition, object source, GameObject instigator)
    {
        _definition = definition;
        _source = source;
        _instigator = instigator;
        
        StatController statController = instigator.GetComponentInChildren<StatController>();
        
        foreach (AbstractGameplayEffectStatModifier modifierDef in definition.ModifierDefinitions)
        {
            StatModifier statModifier;
            if (modifierDef is GameplayEffectDamageDefinition damageDefinition)
            {
                float calculatedMagnitude = modifierDef.Formula.CalculateValue(instigator);
                HealthModifier healthModifier = new HealthModifier
                {
                    Magnitude = Mathf.RoundToInt(calculatedMagnitude),
                    IsCriticalHit = false
                };
                if (damageDefinition.CanCriticalHit)
                {
                    if (statController.Stats["CritChance"].Value / 100f >= Random.value)
                    {
                        healthModifier.Magnitude = Mathf.RoundToInt(healthModifier.Magnitude * (1 + (statController.Stats["CritMultiplier"].Value / 100f)));
                        healthModifier.IsCriticalHit = true;
                    }
                }
                statModifier = healthModifier;
            }
            else
            {
                float magnitude = 0;
                if (modifierDef.Formula.Graph)
                {
                    magnitude = modifierDef.Formula.Graph.CalculateValue(instigator);
                }
                else
                {
                    magnitude = modifierDef.Formula.StaticValue;
                }
                statModifier = new StatModifier()
                {
                    Magnitude = Mathf.RoundToInt(magnitude)
                };
            }

            statModifier.Source = this;
            statModifier.Type = modifierDef.Type;
            _modifiers.Add(statModifier);
        }
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
