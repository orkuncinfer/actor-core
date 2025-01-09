using System;
using System.Collections;
using System.Collections.Generic;
using Core.Editor;
using LevelSystem;
using SaveSystem.Scripts.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace StatSystem
{
    public class StatController : MonoInitializable, ISavable
    {
        public StatDataBase DataBase;

        protected Dictionary<string, Stat> _stats = new Dictionary<string, Stat>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Stat> Stats => _stats;

        public event Action onInitialized;
        public event Action onWillUninitialize;
        public event Action<Stat> onStatIsModified;

        [ShowInInspector]public List<Attribute> AttributeList = new List<Attribute>(); 
        [ShowInInspector]public List<Stat> StatList = new List<Stat>();
        [ShowInInspector]public List<PrimaryStat> PrimaryStatList = new List<PrimaryStat>();
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
            if (_stats[name] is Attribute attribute)
            {
                DDebug.Log(attribute.CurrentValue.ToString());
                return attribute;
            }

            return null;
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
                        if (_stats.TryGetValue(statNode.StatName.Trim(), out Stat stat))//formuldeki herhangi bir stat değiştiğinde formülün sahibini tekrar hesaplar
                        {
                            statNode.Stat = stat;
                            stat.onStatValueChanged += currentStat.CalculateStatValue;
                        }
                        else
                        {
                            Debug.LogWarning($"Stat {statNode.StatName.Trim()} does not exist!");
                        }
                    }
                    
                    /*List<LevelNode> levelNodes = currentStat.Definition.Formula.FindNodesOfType<LevelNode>();
                    foreach (var levelNode in levelNodes)
                    {
                        GetComponent<ILevelable>().levelChanged += currentStat.CalculateValue;
                        Debug.Log("has level");
                    }*/
                }
            }
        }

        public override void  Initialize()
        {
            foreach (StatDefinition definition in DataBase.Stats)
            {
               
                Stat stat = new Stat(definition,this);
                _stats.Add(definition.name,stat);
                StatList.Add(stat);
                stat.onStatValueChanged += () => StatIsModified(stat);
            }
            foreach (StatDefinition definition in DataBase.PrimaryStats)
            {
                PrimaryStat primaryStat = new PrimaryStat(definition,this);
                _stats.Add(definition.name,primaryStat);
                PrimaryStatList.Add(primaryStat);
                primaryStat.onStatValueChanged += () => StatIsModified(primaryStat);
            }
            foreach (StatDefinition definition in DataBase.Attributes)
            {
                Attribute attribute = new Attribute(definition,this);
                _stats.Add(definition.name,attribute);
                AttributeList.Add(attribute);
                attribute.onCurrentValueChanged += () => StatIsModified(attribute);
            }
            InitializeStatFormula();

            foreach (Stat stat in _stats.Values)
            {
                stat.Initialize();  
            }
            onInitialized?.Invoke();
            IsInitialized = true;
        }

        private void StatIsModified(Stat stat)
        {
            //Debug.Log("Stat is modified : " + stat.Definition.name + " : " + stat.Value);
            onStatIsModified?.Invoke(stat);
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
                        stats.Add(stat.Definition.name,savable.data);
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
            StatControllerData statControllerData = (StatControllerData) data;
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

