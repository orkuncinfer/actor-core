using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
using Sirenix.OdinInspector;
using StatSystem;
using UnityEngine;

public class AnimancerController : MonoBehaviour
{
    [SerializeField] private AnimancerComponent _animancerComponent;
    [SerializeField] private AbilityController _abilityController;
    
    private List<AbilityWindowAction> _abilityWindowActions = new List<AbilityWindowAction>();
    private List<AbilityWindowAction> _abilityWindowActionsToRemove = new List<AbilityWindowAction>();
    private ActiveAbility _lastActivatedAbility;
    private Actor _owner;
    private bool _isLooping;
    private float _loopRemainingTime;
    private void Start()
    {
        _abilityController.onActivatedAbility += OnActivatedAbility;
        _owner = _abilityController.GetComponent<Actor>();
    }

    private void Update()
    {
        _abilityWindowActions.ForEach(action => action.Action.OnTick(_owner));
        
        _loopRemainingTime -= Time.deltaTime;
        if(_loopRemainingTime <= 0)
        {
            _isLooping = false;
            OnEnd();
        }
    }


    private void OnActivatedAbility(ActiveAbility ability)
    {
        _isLooping = ability.Definition.IsLoopingAbility;
        _loopRemainingTime = ability.Definition.Duration;
        _lastActivatedAbility = ability;
        if (ability.Definition.AnimationClip)
        {
            _animancerComponent.Stop();
            _animancerComponent.Play(ability.Definition.AnimationClip);
            
            if (ability.Definition.IsBasicAttack)
            {
                float attackSpeedStat = _abilityController.GetComponent<StatController>().Stats["AttackSpeed"].Value;
                _animancerComponent.States.Current.Speed = (attackSpeedStat/100f) / (1 / ability.Definition.AnimationClip.length);
            }
            else
            {
                if (ability.Definition.OverrideAnimSpeed)
                {
                    _animancerComponent.States.Current.Speed = ability.Definition.AnimationSpeed / (1 / ability.Definition.AnimationClip.length);
                }
            }

            _animancerComponent.States.Current.Events.OnEnd += OnEnd;
            
        }
        else
        {
            Cast();
        }
        
    }
    [Button]
    public void CancelCurrent()
    {
        _isLooping = false;
        //_animancerComponent.States.Current.Stop();
        OnEnd();
    }

    private void OnEnd()
    {
        if (_isLooping)
        {
            _animancerComponent.States.Current.Time = 0;
            Debug.Log("looping return");
            return;   
        }
        
        _animancerComponent.States.Current.Events.OnEnd -= OnEnd;
        if (_abilityController.CurrentAbility is ActiveAbility activeAbility)
        {
            foreach (GameplayTag gameplayTag in activeAbility.Definition.GrantedTagsDuringAbility)
            {
                _abilityController.GetComponent<TagController>().RemoveTag(gameplayTag.FullTag);
            }
        }
        
        foreach (AbilityWindowAction abilityWindowAction in _abilityWindowActions)
        {
            abilityWindowAction.Action.OnExit(_owner);
        }
        _abilityWindowActions.Clear();
    }

    public void Cast()
    {
        if (_abilityController.CurrentAbility is SingleTargetAbility singleTargetAbility)
        {
            singleTargetAbility.Cast(_abilityController.Target);
        }
        if (_abilityController.CurrentAbility is ProjectileAbility projectileAbility)
        {
            projectileAbility.Shoot(_abilityController.Target);
        }
    }
    
    public void AnimEvent(string eventName)
    {
        if (eventName.EndsWith("_Start", StringComparison.OrdinalIgnoreCase))
        {
            int index = eventName.IndexOf("_Start", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                 string newEventName = eventName.Substring(0, index);
                 foreach (AbilityWindowAction windowAction in _lastActivatedAbility.Definition.AbilityWindowActions)
                 {
                     if (windowAction.EventName.Equals(newEventName,StringComparison.OrdinalIgnoreCase))
                     {
                         _abilityWindowActions.Add(windowAction);
                         windowAction.Action.OnStart(_owner,_lastActivatedAbility);
                     }
                 }
            }
        }
        
        if (eventName.EndsWith("_End", StringComparison.OrdinalIgnoreCase))
        {
            int index = eventName.IndexOf("_End", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                string newEventName = eventName.Substring(0, index);
                foreach (AbilityWindowAction windowAction in _lastActivatedAbility.Definition.AbilityWindowActions)
                {
                    if (windowAction.EventName.Equals(newEventName,StringComparison.OrdinalIgnoreCase) && _abilityWindowActions.Contains(windowAction))
                    {
                        windowAction.Action.OnExit(null);
                        _abilityWindowActions.Remove(windowAction);
                    }
                }
            }
        }
    }
}
