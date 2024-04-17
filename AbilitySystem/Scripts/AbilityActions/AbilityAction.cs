using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class AbilityAction
{
    public string EventName; 
    public virtual AbilityAction Clone()
    {
        return new AbilityAction
        {
            EventName = this.EventName
        };
    }
    
    public virtual void Reset()
    {
        EventName = null;
    }
    public virtual void OnStart(Actor owner, ActiveAbility ability)
    {
        
    }
    public virtual void OnExit(Actor owner)
    {
        
    }
    public virtual void OnTick(Actor owner)
    {
        
    }
}
