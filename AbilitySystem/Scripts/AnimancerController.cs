using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
using Sirenix.OdinInspector;
using StatSystem;
using UnityEngine;
using Object = UnityEngine.Object;

public class AnimancerController : MonoBehaviour
{
    private AnimancerComponent _animancerComponent;
    private AbilityController _abilityController;
    
    private readonly List<AbilityAction> _registeredAbilityActions = new List<AbilityAction>();
    private List<AbilityAction> _abilityWindowActionsToRemove = new List<AbilityAction>();
    private ActiveAbility _lastActivatedAbility;
    private Actor _owner;
    private bool _isLooping;
    private float _loopRemainingTime;
    private void Start()
    {
        _owner = transform.root.GetComponent<Actor>();
        _abilityController = _owner.GetData<Data_Character>().AbilityController;
        _animancerComponent = GetComponent<AnimancerComponent>();
        
        _abilityController.onActivatedAbility += OnActivatedAbility;
    }

    private void Update()
    {
        for (int i = 0; i < _registeredAbilityActions.Count; i++)
        {
            _registeredAbilityActions[i].OnTick(_owner);
        }
        
        if(_loopRemainingTime > 0)
        {
            _loopRemainingTime -= Time.deltaTime;
            if(_loopRemainingTime <= 0)
            {
                _isLooping = false;
                OnEnd();
            }
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
            DDebug.Log("looping return");
            return;   
        }
        
        _animancerComponent.States.Current.Events.OnEnd -= OnEnd;
        if (_abilityController.CurrentAbility is ActiveAbility activeAbility)
        {
            foreach (GameplayTag gameplayTag in activeAbility.Definition.GrantedTagsDuringAbility)
            {
                _abilityController.GetComponent<TagController>().RemoveTag(gameplayTag);
            }
        }
        
        foreach (AbilityAction abilityWindowAction in _registeredAbilityActions)
        {
            abilityWindowAction.OnExit(_owner);
        }
        _registeredAbilityActions.Clear();
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
                 foreach (AbilityAction windowAction in _lastActivatedAbility.Definition.AbilityActions)
                 {
                     if (windowAction.EventName.Equals(newEventName,StringComparison.OrdinalIgnoreCase))
                     {
                         AbilityAction abilityAction = windowAction.Clone();
                         _registeredAbilityActions.Add(abilityAction);
                         abilityAction.OnStart(_owner,_lastActivatedAbility);
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
                foreach (AbilityAction abilityAction in _lastActivatedAbility.Definition.AbilityActions)
                {
                    if (abilityAction.EventName.Equals(newEventName,StringComparison.OrdinalIgnoreCase) && _registeredAbilityActions.Contains(abilityAction))
                    {
                        abilityAction.OnExit(null);
                        _registeredAbilityActions.Remove(abilityAction);
                    }
                }
            }
        }
    }
}
