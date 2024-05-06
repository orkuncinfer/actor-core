using System;
using System.Collections;
using System.Collections.Generic;
using SaveSystem.Scripts.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StatSystem
{
    [System.Serializable]
    public class Attribute : Stat, ISavable
    {
        [ShowInInspector]protected int _currentValue;
        public int CurrentValue => _currentValue;

        public event Action onCurrentValueChanged;
        public event Action<int,int> onAttributeChanged;
        public event Action<StatModifier> onAppliedModifier;
    
        public Attribute(StatDefinition definition, StatController controller) : base(definition, controller)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _currentValue = Value;
        }

        public virtual void ApplyModifier(StatModifier modifier)
        {
            int newValue = _currentValue;
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
            if(newValue > BaseValue) newValue = BaseValue;
            if (CurrentValue != newValue)
            {
                int oldValue = _currentValue;
                _currentValue = newValue;
                onCurrentValueChanged?.Invoke();
                
                onAttributeChanged?.Invoke(oldValue,_currentValue);
            }
            onAppliedModifier?.Invoke(modifier);
        }

        #region SaveSystem

        public object data => new AttributeData
        {
            CurrentValue = this.CurrentValue
        };
        
        public void Load(object data)
        {
            AttributeData attribute = (AttributeData) data;
            _currentValue = attribute.CurrentValue;
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

