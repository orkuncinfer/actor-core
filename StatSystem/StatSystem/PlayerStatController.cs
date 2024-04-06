using System;
using System.Collections;
using System.Collections.Generic;
using Core.Editor;
using LevelSystem;
using StatSystem;
using UnityEngine;

public class PlayerStatController : StatController
{
        protected ILevelable _levelable;

        protected int _statPoints = 5;
        public event Action onStatPointsChanged;

        public int StatPoints
        {
            get => _statPoints;
            internal set
            {
                _statPoints = value;
                onStatPointsChanged?.Invoke();
            }
        }

        protected override void Awake()
        {
            _levelable = GetComponent<ILevelable>();
        }

        private void OnEnable()
        {
            _levelable.initialized += OnLevelableInitialized;
            _levelable.willUninitialize += UnregisterEvents;
            if (_levelable.isInitialized)
            {
                OnLevelableInitialized();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                StatPoints++;
            }
        }
        private void OnDisable()
        {
            _levelable.initialized -= OnLevelableInitialized;
            _levelable.willUninitialize -= UnregisterEvents;
            if (_levelable.isInitialized)
            {
                UnregisterEvents();
            }
        }

        private void OnLevelableInitialized()
        {
            Initialize();
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            _levelable.levelChanged += OnLevelChanged;
        }
        
        private void UnregisterEvents()
        {
            _levelable.levelChanged -= OnLevelChanged;
        }

        private void OnLevelChanged()
        {
            StatPoints += 5;
        }

        protected override void InitializeStatFormula()
        {
            base.InitializeStatFormula();
            foreach (Stat currentStat in _stats.Values)
            {
                if (currentStat.Definition.Formula != null && currentStat.Definition.Formula.RootNode != null)
                {
                    List<LevelNode> levelNodes = currentStat.Definition.Formula.FindNodesOfType<LevelNode>();
                    foreach (LevelNode levelNode in levelNodes)
                    {
                        levelNode.levelable = _levelable;
                        _levelable.levelChanged += currentStat.CalculateValue;
                    }
                }
            }
        }


        #region Stat System

        public override object data
        {
            get
            {
                return new PlayerStatControllerData(base.data as StatControllerData)
                {
                    statPoints = _statPoints
                };
            }
        }

        public override void Load(object data)
        {
            base.Load(data);
            PlayerStatControllerData playerStatControllerData = (PlayerStatControllerData)data;
            _statPoints = playerStatControllerData.statPoints;
            onStatPointsChanged?.Invoke();
        }

        [Serializable]
        protected class PlayerStatControllerData : StatControllerData
        {
            public int statPoints;

            public PlayerStatControllerData(StatControllerData statControllerData)
            {
                Stats = statControllerData.Stats;
            }
        }

        #endregion
}
