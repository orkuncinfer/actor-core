using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
[Serializable]
public class AbilityAction
{
    public enum EAbilityActionActivationPolicy
    {
        AnimWindow,
        AnimEvent,
        Lifetime
    }
    
    public bool UseAnimEvent => ActivationPolicy == EAbilityActionActivationPolicy.AnimEvent;
    public bool UseAnimWindow => ActivationPolicy == EAbilityActionActivationPolicy.AnimWindow;
    
    public EAbilityActionActivationPolicy ActivationPolicy;
    
    [ShowIf("UseAnimEvent")]public string EventName;
    [MinMaxSlider(0,1)][ShowIf("UseAnimWindow")]public Vector2 AnimWindow;

    private bool _isRunning;
    public bool IsRunning => _isRunning;
    
    private bool _hasExecutedOnce ;
    public bool HasExecutedOnce  => _hasExecutedOnce;

    protected bool _hasTick;
    public bool HasTick => _hasTick;
   
    public virtual AbilityAction Clone()
    {
        _hasExecutedOnce = false;
        _isRunning = false;
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
        _isRunning = true;
        _hasExecutedOnce = true;
    }
    public virtual void OnExit()
    {
        _isRunning = false;
    }
    public virtual void OnTick(Actor owner)
    {
        
    }
}
