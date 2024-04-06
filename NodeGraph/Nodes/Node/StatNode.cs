using System;
using StatSystem;
using UnityEngine;

namespace Core.Editor
{
    public class StatNode : CodeFunctionNode
    {
        [SerializeField] private float _value;
        public override float Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnValidateSelf();
                }
            }
        }

        public override float CalculateValue(GameObject source)
        {
            StatController statController = source.GetComponent<StatController>();
            return statController.Stats[StatName].Value;
        }

        public string StatName;
        public Stat Stat;

        private void OnEnable()
        {
            RefreshValue();
        }

        private void OnDisable()
        {
            RefreshValue();
        }

        public void RefreshValue()
        {
            float tempVal = 0;
            if (Stat != null && Stat.Definition != null)
            {
                tempVal = Stat.BaseValue;
            }

            Value = tempVal;
            OnValidateSelf();
        }
       
#if UNITY_EDITOR
        private void OnValidate()
        {
            OnValidateSelf();
        }
#endif
    }
}