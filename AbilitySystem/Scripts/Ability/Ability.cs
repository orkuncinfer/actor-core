using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SaveSystem.Scripts.Runtime;
using UnityEngine;

public abstract class Ability : ISavable
{
    protected AbilityDefinition _abilityDefinition;
    public AbilityDefinition AbilityDefinition => _abilityDefinition;

    protected AbilityController _controller;

    public Actor Owner;
    public event Action levelChanged;
    private int m_Level = 1;

    public int level
    {
        get => m_Level;
        internal set
        {
            int newLevel = Mathf.Min(value, AbilityDefinition.MaxLevel);
            if (newLevel != m_Level)
            {
                m_Level = newLevel;
                levelChanged?.Invoke();
            }
        }
    }
    
    public Ability(AbilityDefinition definition, AbilityController controller)
    {
        Owner = controller.GetComponent<Actor>();
        _abilityDefinition = definition;
        _controller = controller;
    }

    internal void ApplyEffects(GameObject other)
    {
        ApplyEffectsInternal(_abilityDefinition.GameplayEffectDefinitions,other);
    }

    internal void ApplyEffectsToSelf()
    {
        if (_abilityDefinition is ActiveAbilityDefinition activeAbilityDefinition)
        {
            ApplyEffectsInternal(activeAbilityDefinition.GrantedEffectsDuringAbility, _controller.gameObject);
        }
    }
    internal void RemoveOngoingEffects()
    {
        if (_abilityDefinition is ActiveAbilityDefinition activeAbilityDefinition)
        {
            GameplayEffectController effectController = _controller.GetComponent<GameplayEffectController>();
            foreach (GameplayEffectDefinition effectDefinition in activeAbilityDefinition.GrantedEffectsDuringAbility)
            {
                effectController.RemoveEffectWithDefinition(effectDefinition);
            }
        }
    }

    private void ApplyEffectsInternal(List<GameplayEffectDefinition> effectDefinitions, GameObject other)
    {
        GameplayEffectController effectController = other.GetComponentInChildren<GameplayEffectController>();

        foreach (GameplayEffectDefinition effectDefinition in effectDefinitions)
        {
            EffectTypeAttribute attribute = effectDefinition.GetType().GetCustomAttributes(true)
                .OfType<EffectTypeAttribute>().FirstOrDefault();
            GameplayEffect effect =
                Activator.CreateInstance(attribute.type, effectDefinition, this, _controller.gameObject) as
                    GameplayEffect;
            effectController.ApplyGameplayEffectToSelf(effect);
        }
        
    }
    
    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (GameplayEffectDefinition effectDefinition in AbilityDefinition.GameplayEffectDefinitions)
        {
            EffectTypeAttribute attribute = effectDefinition.GetType().GetCustomAttributes(true)
                .OfType<EffectTypeAttribute>().FirstOrDefault();
            GameplayEffect effect =
                Activator.CreateInstance(attribute.type, effectDefinition, this, _controller.gameObject) as
                    GameplayEffect;
            stringBuilder.Append(effect).AppendLine();
        }

        return stringBuilder.ToString();
    }

    #region SaveSystem

    public object data => new AbilityData
    {
        Level = m_Level
    };
    public void Load(object data)
    {
        AbilityData abilityData = (AbilityData) data;
        level = abilityData.Level;
        levelChanged?.Invoke();
    }
    [Serializable]
    protected class AbilityData
    {
        public int Level;
    }

    #endregion

    
}