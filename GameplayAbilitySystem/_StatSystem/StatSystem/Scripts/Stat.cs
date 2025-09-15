using System;
using System.Collections.Generic;
using Core.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StatSystem
{
    [System.Serializable]
    public class Stat
    {
        public StatDefinition Definition { get; set; }
        public float Value { get; private set; }
        public virtual float BaseValue => Definition.BaseValue;
        
        public event Action<float, float> onValueChangedWithArgs;
        public event Action onStatValueChanged;
        
        protected List<StatModifier> _modifiers = new List<StatModifier>();
        protected StatController _controller;

        public Stat()
        {
        }
        
        public Stat(StatDefinition definition, StatController controller)
        {
            Definition = definition;
            _controller = controller;
        }

        public virtual void Initialize()
        {
            RegisterToSubStats();
            CalculateStatValue();
        }
        
        public virtual void SetValue(float value)
        {
            float oldValue = Value;
            Value = value;
            
            if (!Mathf.Approximately(oldValue, Value))
            {
                onValueChangedWithArgs?.Invoke(oldValue, Value);
                onStatValueChanged?.Invoke();
            }
        }

        private void RegisterToSubStats()
        {
            if (Definition.Formula != null && Definition.Formula.RootNode != null)
            {
                foreach (var node in Definition.Formula.Nodes)
                {
                    if (node is StatNode statNode)
                    {
                        Debug.Log($"Registered to change of stat: {statNode.StatName} for stat: {Definition.name}");
                        _controller.GetStat(statNode.StatName).onStatValueChanged += CalculateStatValue;
                    }
                    else if (node is LevelNode levelNode)
                    {
                        // Level node handling remains as is
                    }
                }
            }
        }

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            CalculateStatValue();
        }
        
        public void RemoveModifierFromSource(object source)
        {
            foreach (var mod in _modifiers)
            {
                if (mod.Source == source)
                {
                    Debug.Log($"Removing modifier: {mod.Magnitude} from source: {mod.Source}");
                }
            }
            
            int numRemoved = _modifiers.RemoveAll(modifier => modifier.Source == source);
            if (numRemoved > 0)
            {
                CalculateStatValue();
            }
        }
        
        internal void CalculateStatValue()
        {
            float finalValue = BaseValue;

            if (Definition.Formula != null && Definition.Formula.RootNode != null)
            {
                finalValue += Definition.Formula.RootNode.CalculateValue(_controller.gameObject);
            }
            
            _modifiers.Sort((x, y) => x.Type.CompareTo(y.Type));
            
            for (int i = 0; i < _modifiers.Count; i++)
            {
                StatModifier modifier = _modifiers[i];
                if (modifier.Type == ModifierOperationType.Additive)
                {
                    finalValue += modifier.Magnitude;
                }
                else if (modifier.Type == ModifierOperationType.Multiplicative)
                {
                    finalValue *= modifier.Magnitude;
                }
            }
            
            if (Definition.Cap >= 0)
            {
                finalValue = Mathf.Min(finalValue, Definition.Cap);
            }
            
            if (!Mathf.Approximately(Value, finalValue))
            {
                Debug.Log($"Stat changed: {Definition.name}");
                float oldValue = Value;
                Value = finalValue;
                onValueChangedWithArgs?.Invoke(oldValue, Value);
                onStatValueChanged?.Invoke();
            }
            else
            {
                Value = finalValue;
            }
        }
    }
}