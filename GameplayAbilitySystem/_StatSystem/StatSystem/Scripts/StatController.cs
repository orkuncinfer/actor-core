using System;
using System.Collections.Generic;
using Core.Editor;
using LevelSystem;
using SaveSystem.Scripts.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StatSystem
{
    public class StatController : MonoBehaviour, ISavable
    {
        public StatDataBase DataBase;

        protected readonly Dictionary<string, Stat> _stats = new Dictionary<string, Stat>();
        
        public Dictionary<string, Stat> Stats => _stats;

        public event Action onInitialized;
        public event Action onWillUninitialize;
        public event Action<Stat> onStatIsModified;

        [ShowInInspector] public List<Attribute> AttributeList = new List<Attribute>(); 
        [ShowInInspector] public List<Stat> StatList = new List<Stat>();
        [ShowInInspector] public List<PrimaryStat> PrimaryStatList = new List<PrimaryStat>();

        public bool IsInitialized;
        
        protected virtual void Awake()
        {
            if (!IsInitialized)
            {
                Initialize();
            }
            
         
        }

        [Button]
        public Attribute GetAttribute(string name)
        {
            if (_stats.TryGetValue(name, out Stat stat) && stat is Attribute attribute)
            {
                return attribute;
            }
            return null;
        }
        
        public Stat GetStat(string name)
        {
            if (_stats.TryGetValue(name, out Stat stat))
            {
                return stat;
            }
            return null;
        }

        public void ResetAttributesToMax()
        {
            foreach (var attribute in AttributeList)
            {
                attribute.SetToMaxValue();
            }
        }

        private void OnDestroy()
        {
            onWillUninitialize?.Invoke();
        }

        protected virtual void InitializeStatFormula()
        {
            foreach (Stat currentStat in _stats.Values)
            {
                if (currentStat.Definition.Formula != null && currentStat.Definition.Formula.RootNode != null)
                {
                    List<StatNode> statNodes = currentStat.Definition.Formula.FindNodesOfType<StatNode>();
                    foreach (var statNode in statNodes)
                    {
                        if (_stats.TryGetValue(statNode.StatName.Trim(), out Stat stat))
                        {
                            statNode.Stat = stat;
                            stat.onStatValueChanged += currentStat.CalculateStatValue;
                        }
                        else
                        {
                            Debug.LogWarning($"Stat {statNode.StatName.Trim()} does not exist!");
                        }
                    }
                }
            }
        }

        public void Initialize()
        {
            if (IsInitialized) return;
            
            foreach (StatDefinition definition in DataBase.Stats)
            {
                Stat stat = new Stat(definition, this);
                _stats.Add(definition.name, stat);
                StatList.Add(stat);
                stat.onStatValueChanged += () => StatIsModified(stat.Definition.name, stat.Value);
            }
            
            foreach (StatDefinition definition in DataBase.PrimaryStats)
            {
                PrimaryStat primaryStat = new PrimaryStat(definition, this);
                _stats.Add(definition.name, primaryStat);
                PrimaryStatList.Add(primaryStat);
                primaryStat.onStatValueChanged += () => StatIsModified(primaryStat.Definition.name, primaryStat.Value);
            }
            
            foreach (StatDefinition definition in DataBase.Attributes)
            {
                Attribute attribute = new Attribute(definition, this);
                _stats.Add(definition.name, attribute);
                AttributeList.Add(attribute);
                attribute.onCurrentValueChanged += () => StatIsModified(attribute.Definition.name, attribute.CurrentValue);
            }
            
            InitializeStatFormula();

            foreach (Stat stat in _stats.Values)
            {
                stat.Initialize();  
            }
            
            ResetAttributesToMax();
            
            onInitialized?.Invoke();
            IsInitialized = true;
        }
        
        private void StatIsModified(string statName, float value)
        {
            onStatIsModified?.Invoke(GetStat(statName));
        }

        #region SaveSystem

        public virtual object data
        {
            get
            {
                Dictionary<string, object> stats = new Dictionary<string, object>();
                foreach (Stat stat in _stats.Values)
                {
                    if (stat is ISavable savable)
                    {
                        stats.Add(stat.Definition.name, savable.data);
                    }
                }

                return new StatControllerData
                {
                    Stats = stats
                };
            }
        }
        
        public virtual void Load(object data)
        {
            StatControllerData statControllerData = (StatControllerData)data;
            foreach (Stat stat in _stats.Values)
            {
                if (stat is ISavable savable)
                {
                    savable.Load(statControllerData.Stats[stat.Definition.name]);
                }
            }
        }

        [Serializable]
        protected class StatControllerData
        {
            public Dictionary<string, object> Stats;
        }

        #endregion
    }
}