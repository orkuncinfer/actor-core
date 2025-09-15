using System;
using System.Collections.Generic;
using SaveSystem.Scripts.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StatSystem
{
    [System.Serializable]
    public class Attribute : Stat, ISavable // Health, Mana, Stamina
    {
        [ShowInInspector] protected float _currentValue;
        
        public float CurrentValue
        {
            get
            {
                if (_tempModifiers.Count > 0)
                {
                    return RecalculateCurrentValueWithTempModifiers();
                }
                return _currentValue;
            }
            set
            {
                Debug.Log("Current value is set to :" + value);
                float oldValue = _currentValue;
                _currentValue = value;
                if (!Mathf.Approximately(_currentValue, oldValue))
                {
                    onCurrentValueChanged?.Invoke();
                    onAttributeChanged?.Invoke(oldValue, _currentValue);
                }
            }
        }

        private StatController _statController;
        public StatController StatController => _statController;

        private Stat _maxValueStat;

        public event Action onCurrentValueChanged;
        public event Action<float, float> onAttributeChanged;
        public event Action<StatModifier, StatController, float> onAppliedModifier;
        
        private List<StatModifier> _tempModifiers = new List<StatModifier>();
    
        public Attribute(StatDefinition definition, StatController controller) : base(definition, controller)
        {
            _statController = controller;
        }

        public override void Initialize()
        {
            base.Initialize();
            _currentValue = Value;
        }
        
        private float RecalculateCurrentValueWithTempModifiers()
        {
            if (Mathf.Approximately(_currentValue, 0f)) return 0f;
            
            float newValue = _currentValue;
            _tempModifiers.Sort((x, y) => x.Type.CompareTo(y.Type));
   
            for (int i = 0; i < _tempModifiers.Count; i++)
            {
                StatModifier modifier = _tempModifiers[i];
                if (modifier.Type == ModifierOperationType.Override)
                {
                    newValue = modifier.Magnitude;
                }
                else if (modifier.Type == ModifierOperationType.Additive)
                {
                    newValue += modifier.Magnitude;
                }
                else if (modifier.Type == ModifierOperationType.Multiplicative)
                {
                    newValue *= modifier.Magnitude;
                }
            }
   
            if (Definition.Cap >= 0)
            {
                newValue = Mathf.Min(newValue, Definition.Cap);
            }
            
            return newValue;
        }
        
        public void ApplyTempModifier(StatModifier modifier)
        {
            _tempModifiers.Add(modifier);
            float oldValue = _currentValue;
            float newValue = RecalculateCurrentValueWithTempModifiers();
            
            if (!Mathf.Approximately(oldValue, newValue))
            {
                onCurrentValueChanged?.Invoke();
                onAttributeChanged?.Invoke(oldValue, newValue);
            }
        }
        
        public void RemoveTempModifier(StatModifier modifier)
        {
            if (_tempModifiers.Contains(modifier))
            {
                _tempModifiers.Remove(modifier);
            }
            
            float oldValue = _currentValue;
            float newValue = RecalculateCurrentValueWithTempModifiers();
            
            if (!Mathf.Approximately(oldValue, newValue))
            {
                onCurrentValueChanged?.Invoke();
                onAttributeChanged?.Invoke(oldValue, newValue);
            }
        }

        public void SetToMaxValue()
        {
            TryGetMaxValueStat();
            if (_maxValueStat != null)
            {
                CurrentValue = _maxValueStat.Value;
            }
            else
            {
                Debug.LogWarning($"{Definition.name} doesn't have a max Stat");
            }
        }

        public Stat TryGetMaxValueStat()
        {
            string maxStatName = "Max_"+ Definition.name;
            Stat maxStat = _statController.GetStat(maxStatName);
            _maxValueStat = maxStat;
            return _maxValueStat;
        }
        
        public virtual float ApplyModifier(StatModifier modifier, bool asServer = true)
        {
            float newValue = CurrentValue;
            float diff = 0f;
            
            Debug.Log($"Modifier is: {modifier.Magnitude} {modifier.Type} CurrentValue: {newValue} BaseValue: {Value}");
            
            switch (modifier.Type)
            {
                case ModifierOperationType.Override:
                    newValue = modifier.Magnitude;
                    break;
                case ModifierOperationType.Additive:
                    newValue += modifier.Magnitude;
                    break;
                case ModifierOperationType.Multiplicative:
                    newValue *= modifier.Magnitude;
                    break;
            }
           
            if (newValue < 0f) newValue = 0f;
            
            if (Definition.Cap >= 0)
            {
                newValue = Mathf.Min(newValue, Definition.Cap);
            }

            if (_maxValueStat != null)
            {
                newValue = Mathf.Min(newValue, _maxValueStat.Value);
            }
            
            if (!Mathf.Approximately(_currentValue, newValue) && asServer)
            {
                float oldValue = _currentValue;
                _currentValue = newValue;
                diff = _currentValue - oldValue;
                onCurrentValueChanged?.Invoke();
                onAttributeChanged?.Invoke(oldValue, _currentValue);
            }
            
            Debug.Log($"Applied value is: CurrentValue: {_currentValue} BaseValue: {Value}");
            onAppliedModifier?.Invoke(modifier, _statController, diff);
            
            return diff;
        }

        public override void SetValue(float value)
        {
            base.SetValue(value);
            _currentValue = value;
            Debug.Log("Current value is set to :" + _currentValue);
        }

        #region SaveSystem

        public object data => new AttributeData
        {
            CurrentValue = this._currentValue,
        };
        
        public void Load(object data)
        {
            if (Definition.Formula != null) return;
            
            AttributeData attribute = (AttributeData)data;
            _currentValue = attribute.CurrentValue;
            Debug.Log($"{Definition.name} is set to value {_currentValue}");
            onCurrentValueChanged?.Invoke();
        }
        
        [Serializable]
        protected class AttributeData
        {
            public float CurrentValue;
        }
        
        #endregion
    }
}