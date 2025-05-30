using System;
using SaveSystem.Scripts.Runtime;
using UnityEngine;

namespace StatSystem
{
    [System.Serializable]
    public class PrimaryStat : Stat, ISavable // Dex, Str, Int
    {
        private int _baseValue;
        public override int BaseValue => _baseValue;

        public PrimaryStat(StatDefinition definition, StatController controller) : base(definition,controller)
        {
            //CalculateStatValue();
        }

        public override void Initialize()
        {
            _baseValue = Definition.BaseValue;
            base.Initialize();
        }

        internal void Add(int amount)
        {
            if (Value >= Definition.Cap)
            {
                return;
            }
            _baseValue += amount;
            CalculateStatValue();
        }

        internal void Subtract(int amount)
        {
            if (Value <= 0)
            {
                return;
            }
            _baseValue -= amount;
            CalculateStatValue();
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
            CalculateStatValue();
        }
        
        [Serializable]
        protected class PrimaryStatData
        {
            public int BaseValue;
        }
        
        #endregion
    }
}