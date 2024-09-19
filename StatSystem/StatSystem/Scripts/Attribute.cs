using System;
using System.Collections;
using System.Collections.Generic;
using SaveSystem.Scripts.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StatSystem
{
    [System.Serializable]
    public class 
        Attribute : Stat, ISavable
    {
        [ShowInInspector]protected int _currentValue;
        public int CurrentValue
        {
            get
            {
                if (_tempModifiers.Count > 0)
                {
                    return RecalculateCurrentValueWithTempModifiers();
                }
                return _currentValue;
            }
        }

        public event Action onCurrentValueChanged;
        public event Action<int,int> onAttributeChanged;
        public event Action<StatModifier> onAppliedModifier;
        
        List<StatModifier> _tempModifiers = new List<StatModifier>();
    
        public Attribute(StatDefinition definition, StatController controller) : base(definition, controller)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _currentValue = Value;
        }
        private int RecalculateCurrentValueWithTempModifiers()
        {
            if (_currentValue == 0) return 0;
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
   
            if (_definition.Cap >= 0)
            {
                newValue = Mathf.Min(newValue, _definition.Cap);
            }
            return Mathf.RoundToInt(newValue);;
        }
        public void ApplyTempModifier(StatModifier modifier)
        {
            _tempModifiers.Add(modifier);
            int oldValue = _currentValue;
            int newValue = RecalculateCurrentValueWithTempModifiers();
            if(oldValue != newValue)
            {
                onCurrentValueChanged?.Invoke();
                onAttributeChanged?.Invoke(oldValue,newValue);
            }
        }
        public void RemoveTempModifier(StatModifier modifier)
        {
            if (_tempModifiers.Contains(modifier))
            {
                _tempModifiers.Remove(modifier);
            }
            int oldValue = _currentValue;
            int newValue = RecalculateCurrentValueWithTempModifiers();
            if(oldValue != newValue)
            {
                onCurrentValueChanged?.Invoke();
                onAttributeChanged?.Invoke(oldValue,newValue);
            }
        }
        
        public virtual int ApplyModifier(StatModifier modifier)
        {
            float newValue = CurrentValue;
            int diff = 0;
            //Debug.Log("modifier is : " + modifier.Magnitude + modifier.Type + newValue);
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
           
            if (newValue < 0) newValue = 0;
            if (_definition.Cap >= 0)
            {
                newValue = Mathf.Min(newValue, _definition.Cap);
            }
            if (_currentValue != newValue)
            {
                int oldValue = _currentValue;
                _currentValue = Mathf.RoundToInt(newValue);
                diff = _currentValue - oldValue;
                onCurrentValueChanged?.Invoke();
                
                onAttributeChanged?.Invoke(oldValue,_currentValue);
            }

            onAppliedModifier?.Invoke(modifier);
            return diff;
        }
        
        #region SaveSystem

        public object data => new AttributeData
        {
            
            CurrentValue = this._currentValue,
        };
        
        public void Load(object data)
        {
            AttributeData attribute = (AttributeData) data;
            _currentValue = attribute.CurrentValue;
            Debug.Log($"{_definition.name} is set to value {_currentValue}");
            onCurrentValueChanged?.Invoke();
        }
        
        [Serializable]
        protected class AttributeData
        {
            public int CurrentValue;
        }
        
        #endregion
        
    }
}

