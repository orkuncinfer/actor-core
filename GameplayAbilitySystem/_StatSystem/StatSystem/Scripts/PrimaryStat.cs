using System;
using SaveSystem.Scripts.Runtime;
using UnityEngine;

namespace StatSystem
{
    [System.Serializable]
    public class PrimaryStat : Stat, ISavable // Dex, Str, Int
    {
        private float _baseValue;
        public override float BaseValue => _baseValue;

        public PrimaryStat(StatDefinition definition, StatController controller) : base(definition, controller)
        {
        }

        public override void Initialize()
        {
            _baseValue = Definition.BaseValue;
            base.Initialize();
        }

        public void Add(float amount)
        {
            if (Definition.Cap >= 0 && Value >= Definition.Cap)
            {
                return;
            }
            
            _baseValue += amount;
            CalculateStatValue();
        }

        internal void Subtract(float amount)
        {
            if (Value <= 0f)
            {
                return;
            }
            
            _baseValue -= amount;
            CalculateStatValue();
        }

        public override void SetValue(float value)
        {
            base.SetValue(value);
            _baseValue = value;
        }

        #region SaveSystem

        public object data => new PrimaryStatData
        {
            BaseValue = this.BaseValue
        };
        
        public void Load(object data)
        {
            PrimaryStatData statData = (PrimaryStatData)data;
            _baseValue = statData.BaseValue;
            CalculateStatValue();
        }
        
        [Serializable]
        protected class PrimaryStatData
        {
            public float BaseValue;
        }
        
        #endregion
    }
}