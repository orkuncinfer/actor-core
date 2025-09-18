using System;
using StatSystem;
using UnityEngine;

namespace Core.Editor
{
    public class StatNode : CodeFunctionNode
    {
        [SerializeField] private float _value;
        private StatController _statController;
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
            if (_statController == null)
                _statController = source.GetComponentInChildren<StatController>();
            
            Debug.Log("Calculating stat values for " + StatName +"in " + _statController.transform.parent.name);
            
            return _statController.Stats[StatName].Value;
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