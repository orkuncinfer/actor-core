using System;
using LevelSystem;
using StatSystem;
using UnityEngine;

namespace Core.Editor
{
    public class LevelNode : CodeFunctionNode
    {
        public ILevelable levelable;
        public bool RegisteredToLevelEvent;
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
            ILevelable levelable = source.GetComponent<ILevelable>();
            return levelable.level;
        }

        public string StatName;
        public Stat Stat;

        private void OnEnable()
        {
            RefreshValue();
        }

        private void OnDestroy()
        {
            if (RegisteredToLevelEvent)
            {
                if (levelable != null)
                {
                    levelable.levelChanged -= RefreshValue;
                }
            }
        }

        private void OnDisable()
        {
            RefreshValue();
        }

        public void RefreshValue()
        {
            if (!RegisteredToLevelEvent)
            {
                if (levelable != null)
                {
                    levelable.levelChanged += RefreshValue;
                }
            }
            if (levelable == null)
            {
                return;
            }
            Value = levelable.level;
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