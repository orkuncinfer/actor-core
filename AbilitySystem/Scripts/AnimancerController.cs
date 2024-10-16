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

    private readonly List<AbilityAction> _registeredAbilityActionList = new List<AbilityAction>();
    private List<AbilityAction> _abilityWindowActionsToRemove = new List<AbilityAction>();
    private Dictionary<Type, AbilityAction> _abilityActions = new Dictionary<Type, AbilityAction>();
    private ActiveAbility _lastActivatedAbility;
    [ShowInInspector] private ActiveAbility _currentActiveAbility;
    private Actor _owner;
    private bool _isLooping;
    private float _loopRemainingTime;
    private bool _abilityAnimPlaying;

    private AnimancerState _currentAbilityState;
    

    private void Start()
    {
        _owner = transform.root.GetComponent<Actor>();
        _abilityController = _owner.GetData<Data_GAS>().AbilityController;
        _animancerComponent = GetComponent<AnimancerComponent>();

        _abilityController.onActivatedAbility += OnActivatedAbility;
        _abilityController.onCanceledAbility += OnCanceledAbility;
    }

    private void Update()
    {
        for (int i = 0; i < _registeredAbilityActionList.Count; i++)
        {
            if (_registeredAbilityActionList[i].HasTick && _registeredAbilityActionList[i].IsRunning)
                _registeredAbilityActionList[i].OnTick(_owner);
        }

        if (_loopRemainingTime > 0)
        {
            _loopRemainingTime -= Time.deltaTime;
            if (_loopRemainingTime <= 0)
            {
                _isLooping = false;
                OnEnd(_abilityController.LastUsedAbility);
            }
        }

        if (_abilityAnimPlaying)
        {
            // if the ability has a playtime and the animation has reached that time, cancel the ability
            if (_currentActiveAbility.Definition is ActiveAbilityDefinition activeAbilityDefinition)
            {
                if (activeAbilityDefinition.PlayTime < 1)
                {
                    if (_animancerComponent.States.Current.NormalizedTime >= activeAbilityDefinition.PlayTime)
                    {
                        CancelCurrent();
                    }
                }
            }

            if (_currentActiveAbility != null)
            {
                foreach (var action in _currentActiveAbility.Definition.AbilityActions)
                {
                    if(action.ActivationPolicy != AbilityAction.EAbilityActionActivationPolicy.AnimWindow) continue;
                    AbilityAction clonedAction = _abilityActions[action.GetType()];
                    if (action.AnimWindow.x <= _animancerComponent.States.Current.NormalizedTime)
                    {
                        if (!_registeredAbilityActionList.Contains(action) && !clonedAction.IsRunning && !clonedAction.HasExecutedOnce)
                        {
                            clonedAction.OnStart(_owner, _currentActiveAbility);
                        }
                    }

                    if (action.AnimWindow.y <= _animancerComponent.States.Current.NormalizedTime)
                    {
                        if (_abilityActions.ContainsKey(action.GetType()) && clonedAction.IsRunning)
                        {
                            clonedAction.OnExit();
                        }
                    }
                }
            }
        }
    }
    private void OnCanceledAbility(ActiveAbility ability)
    {
        Debug.Log("end1");
        foreach (AbilityAction abilityAction in ability.Definition.AbilityActions) // exit lifetime actions
        {
            Debug.Log("end2");
            if(_abilityActions.ContainsKey(abilityAction.GetType()) == false) continue;
            AbilityAction clonedAction = _abilityActions[abilityAction.GetType()];
            if (abilityAction.ActivationPolicy == AbilityAction.EAbilityActionActivationPolicy.Lifetime)
            {
                Debug.Log("end3");
                clonedAction.OnExit();
                _abilityActions.Remove(clonedAction.GetType());
                _registeredAbilityActionList.Remove(clonedAction);
            }
        }
    }

    private void OnActivatedAbility(ActiveAbility ability)
    {
        _isLooping = ability.Definition.IsLoopingAbility;
        _loopRemainingTime = ability.Definition.Duration;
        _lastActivatedAbility = ability;
        _currentActiveAbility = ability;

        foreach (AbilityAction actionsToClone in ability.Definition.AbilityActions)
        {
            AbilityAction clonedAction = actionsToClone.Clone();
            if(_abilityActions.ContainsKey(clonedAction.GetType()) == false)_abilityActions.Add(clonedAction.GetType(), clonedAction);
            if (actionsToClone.ActivationPolicy == AbilityAction.EAbilityActionActivationPolicy.Lifetime)
            {
                clonedAction.OnStart(_owner, ability);
            }
            if (!_registeredAbilityActionList.Contains(clonedAction))
            {
                _registeredAbilityActionList.Add(clonedAction);
            }
        }

        if (ability.Definition.AnimationClip)
        {
            _animancerComponent.Stop();
            _currentAbilityState = _animancerComponent.Play(ability.Definition.AnimationClip);
            _currentAbilityState.Events.OnEnd = () => OnEnd(ability);
            _abilityAnimPlaying = true;
            if (ability.Definition.IsBasicAttack)
            {
                float attackSpeedStat = _abilityController.GetComponent<StatController>().Stats["AttackSpeed"].Value;
                _animancerComponent.States.Current.Speed =
                    (attackSpeedStat / 100f) / (1 / ability.Definition.AnimationClip.length);
            }
            else
            {
                if (ability.Definition.OverrideAnimSpeed)
                {
                    _animancerComponent.States.Current.Speed = ability.Definition.AnimationSpeed /
                                                               (1 / ability.Definition.AnimationClip.length);
                }
            }
        }
        else
        {
            ReCast(ability);
        }
    }

    [Button]
    public void CancelCurrent()
    {
        _isLooping = false;
        OnEnd(_abilityController.LastUsedAbility);
    }

    private void OnEnd(ActiveAbility ability) // the animation has came to an end
    {
        if (_isLooping)
        {
            _animancerComponent.States.Current.Time = 0;
            return;
        }

        _abilityAnimPlaying = false;
        if (ability is ActiveAbility activeAbility) // possible bug
        {
            foreach (GameplayTag gameplayTag in activeAbility.Definition.GrantedTagsDuringAbility)
            {
                _abilityController.GetComponent<TagController>().RemoveTag(gameplayTag);
            }
        }
        Debug.Log("end1");
        foreach (AbilityAction abilityAction in ability.Definition.AbilityActions) // exit lifetime actions
        {
            Debug.Log("end2");
            AbilityAction clonedAction = _abilityActions[abilityAction.GetType()];
            if (abilityAction.ActivationPolicy == AbilityAction.EAbilityActionActivationPolicy.Lifetime)
            {
                Debug.Log("end3");
                clonedAction.OnExit();
                _abilityActions.Remove(clonedAction.GetType());
                _registeredAbilityActionList.Remove(clonedAction);
            }
        }

        _registeredAbilityActionList.Clear();
        _abilityActions.Clear();

        _abilityController.CancelAbilityIfActive(ability);

        _currentActiveAbility = null;
        _currentAbilityState = null;
    }

    public void ReCast(ActiveAbility ability)
    {
        if (ability is SingleTargetAbility singleTargetAbility)
        {
            singleTargetAbility.Cast(_abilityController.Target);
        }

        if (ability is ProjectileAbility projectileAbility)
        {
            projectileAbility.Shoot(_abilityController.Target);
        }
    }

    public void Cast()
    {
        if (_currentActiveAbility is SingleTargetAbility singleTargetAbility)
        {
            singleTargetAbility.Cast(_abilityController.Target);
        }

        if (_currentActiveAbility is ProjectileAbility projectileAbility)
        {
            projectileAbility.Shoot(_abilityController.Target);
        }
    }

    public void AnimEvent(string eventName)
    {
        if (_currentActiveAbility == null) return;
        if (eventName.EndsWith("_Start", StringComparison.OrdinalIgnoreCase))
        {
            int index = eventName.IndexOf("_Start", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                string newEventName = eventName.Substring(0, index);
                foreach (AbilityAction abilityAction in _currentActiveAbility.Definition.AbilityActions)
                {
                    if (!abilityAction.UseAnimEvent) continue;
                    if (abilityAction.EventName.Equals(newEventName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (_abilityActions.ContainsKey(abilityAction.GetType()))
                        {
                            _abilityActions[abilityAction.GetType()].OnStart(_owner, _currentActiveAbility);
                        }
                        else
                        {
                        }
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
                
                foreach (AbilityAction abilityAction in _currentActiveAbility.Definition.AbilityActions)
                {
                    AbilityAction clonedAction = _abilityActions[abilityAction.GetType()];
                    if (!abilityAction.UseAnimEvent) continue;
                    if (clonedAction.EventName.Equals(newEventName, StringComparison.OrdinalIgnoreCase))
                    {
                        clonedAction.OnExit();
                        _abilityActions.Remove(clonedAction.GetType());
                        _registeredAbilityActionList.Remove(clonedAction);
                    }
                }
            }
        }
    }
}