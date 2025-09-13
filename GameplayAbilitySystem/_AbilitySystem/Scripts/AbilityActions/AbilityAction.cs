using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
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
    
    protected AbilityDefinition Definition;
    
    private bool _isRunning;
    public bool IsRunning => _isRunning;
    
    private bool _hasExecutedOnce ;
    public bool HasExecutedOnce  => _hasExecutedOnce;

    protected bool _hasTick;
    public bool HasTick => _hasTick;
    
    public float TimeLength { get; set; } // duration in ms

    public ActorBase Owner { get; set; }
    public ActiveAbility ActiveAbility {get; set;}
   
    public virtual AbilityAction Clone()
    {
        Debug.Log("Ability cloned " + this.GetType().Name);

        return null;
    }

    protected void RequestEndAbility()
    {
        Owner.GetService<Service_GAS>().AbilityController.CancelAbilityIfActive(ActiveAbility);
    }
    public virtual void Reset()
    {
        Debug.Log("Ability resetted " + this.GetType().Name);
        _hasExecutedOnce = false;
        _isRunning = false;
        EventName = null;
        //_hasExecutedOnce = false;
    }
    public virtual void OnStart()
    {
        Debug.Log($"<color=green>AbilityAction</color> started : {this.GetType().Name} Time : {Time.time}");
        Definition = ActiveAbility.Definition;
        _isRunning = true;
        _hasExecutedOnce = true;
    }
    public virtual void OnExit()
    {
        if(!_isRunning) return;
        _isRunning = false;
    }
    public virtual void OnTick(Actor owner)
    {
        
    }
}
