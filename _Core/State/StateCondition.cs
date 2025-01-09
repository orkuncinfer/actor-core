using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class StateCondition
{
    protected ActorBase Owner;
    
#if UNITY_EDITOR
    private List<string> WatchingValuesList = new List<string>();
#endif
    
    public virtual void Initialize(ActorBase owner)
    {
        Owner = owner;
    }
    public virtual bool CheckCondition(){ return false; }

    public void AddWatchingValue(string value)
    {
        #if UNITY_EDITOR
        if (WatchingValuesList.Contains(value)) return;
        WatchingValuesList.Add(value);
        #endif
    }
#if UNITY_EDITOR
    public List<string> GetWatchingValues()
    {
        return WatchingValuesList;
    }
#endif
}