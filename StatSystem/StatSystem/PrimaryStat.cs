using System;
using SaveSystem.Scripts.Runtime;
using UnityEngine;

namespace StatSystem
{
    public class PrimaryStat : Stat, ISavable
    {
        private int _baseValue;
        public override int BaseValue => _baseValue;

        public PrimaryStat(StatDefinition definition, StatController controller) : base(definition,controller)
        {
            _baseValue = definition.BaseValue;
            CalculateValue();
        }

        internal void Add(int amount)
        {
            if (Value >= Definition.Cap)
            {
                return;
            }
            _baseValue += amount;
            CalculateValue();
        }

        internal void Subtract(int amount)
        {
            if (Value <= 0)
            {
                return;
            }
            _baseValue -= amount;
            CalculateValue();
        }
        
        
        #region SaveSystem

        public object data => new PrimaryStatData
        {
            BaseValue = this.BaseValue
        };
        
        public void Load(object data)
        {
            PrimaryStatData statData = (PrimaryStatData) data;
            _baseValue = statData.BaseValue;
            CalculateValue();
        }
        
        [Serializable]
        protected class PrimaryStatData
        {
            public int BaseValue;
        }
        
        #endregion
    }
}