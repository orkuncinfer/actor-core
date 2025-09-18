using System.Collections;
using System.Collections.Generic;
using Core.Editor;
using Sirenix.OdinInspector;
using StatSystem;
using UnityEngine;

public class GameplayPersistentEffect : GameplayEffect
{
    public new GameplayPersistentEffectDefinition Definition => _definition as GameplayPersistentEffectDefinition;
    public float RemainingDuration;
    
    public float RemainingPeriod;

    [ShowInInspector] private string _name => Definition.name;
    
    private float _duration;
    public float Duration => _duration;
    
    private Dictionary<Stat,List<StatModifier>> _statDependentModifiers = new Dictionary<Stat,List<StatModifier>>();
    
    public GameplayPersistentEffect(GameplayPersistentEffectDefinition definition, object source, GameObject instigator,GameObject victim) : base(definition, source, instigator,victim)
    {
        RemainingPeriod = definition.Period;
        if (!definition.IsInfinite)
        {
            RemainingDuration = _duration = definition.DurationFormula.CalculateValue(instigator);
        }

        _statController.onStatIsModified += OnAnyStatModified; // todo unregister on return pool ?

        foreach (var modifier in Modifiers)
        {
            if (modifier.ModifierDefinition.Formula.Graph != null)
            {
                foreach (var graphNode in modifier.ModifierDefinition.Formula.Graph.Nodes)
                {
                    if (graphNode is StatNode statNode)
                    {
                        Stat stat = _statController.GetStat(statNode.StatName);
                        if (stat != null)
                        {
                            if (!_statDependentModifiers.ContainsKey(stat))
                            {
                                _statDependentModifiers[stat] = new List<StatModifier>();
                            }
                            _statDependentModifiers[stat].Add(modifier);
                        }
                    }
                }
            }
        }
    }

    private void OnAnyStatModified(Stat obj)
    {
        if (_statDependentModifiers.ContainsKey(obj))
        {
            foreach (var statModifier in _statDependentModifiers[obj])
            {
                statModifier.Magnitude =
                    CalculateMagnitude(statModifier.ModifierDefinition, statModifier.Victim as GameObject);
                
                Debug.Log("Recalculated stat: " + obj.Definition.Title +"-"+statModifier.Magnitude);
            }
        }
    }
}
