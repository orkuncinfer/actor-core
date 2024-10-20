using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
using DG.Tweening;
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
    private bool _currentAbilityAnimationReachedFullWeight;

    private AnimancerState _previousAnimStateBeforeAbility;
    private AnimancerState _currentAbilityAnimState;

    private Dictionary<AnimancerState, Action> _onEndActions = new Dictionary<AnimancerState, System.Action>();

    private Action _onAnimationEnd;

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
        if (_currentAbilityAnimState != null)
        {
            
        }
        
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
                OnAnimEnd(_abilityController.LastUsedAbility);
            }
        } 
        for (int i = _abilityController.GetActiveAbilities().Count ; i > 0; i--) // ANIMATION END CHECK
        {
            ActiveAbility ability = _abilityController.GetActiveAbilities()[i - 1];
            if(ability.AnimancerState == null) continue;
            if (ability.AnimancerState.NormalizedTime >= ability.Definition.EndTime)
            {
                _isLooping = false;
                EndOrInterrupted(ability.AnimancerState, ability);
            }
            if (ability.Definition.EndTime < 1)
            { }
                
            if (ability.AnimancerState.EffectiveWeight > 0.9f && !ability.AnimationReachedFullWeight) // ANIMATION INTERRUPT CHECK
            {
                ability.AnimationReachedFullWeight = true;
            }
            if (ability.AnimancerState.EffectiveWeight <= 0 && ability.AnimationReachedFullWeight)
            {
                EndOrInterrupted(_currentAbilityAnimState, _currentActiveAbility);
            }
        }

        if (_abilityAnimPlaying)
        {
            if (_currentActiveAbility != null)
            {
                foreach (var action in _currentActiveAbility.Definition.AbilityActions)
                {
                    if (action.ActivationPolicy != AbilityAction.EAbilityActionActivationPolicy.AnimWindow) continue;
                    AbilityAction clonedAction = _abilityActions[action.GetType()];
                    if (action.AnimWindow.x <= action.ActiveAbility.AnimancerState.NormalizedTime)
                    {
                        if (!_registeredAbilityActionList.Contains(action) && !clonedAction.IsRunning &&
                            !clonedAction.HasExecutedOnce)
                        {
                            clonedAction.OnStart(_owner, _currentActiveAbility);
                        }
                    }

                    if (action.AnimWindow.y <= action.ActiveAbility.AnimancerState.NormalizedTime)
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
        foreach (AbilityAction abilityAction in ability.Definition.AbilityActions) // exit lifetime actions
        {
            if (_abilityActions.ContainsKey(abilityAction.GetType()) == false) continue;
            AbilityAction clonedAction = _abilityActions[abilityAction.GetType()];
            if (abilityAction.ActivationPolicy == AbilityAction.EAbilityActionActivationPolicy.Lifetime)
            {
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
            if (_abilityActions.ContainsKey(clonedAction.GetType()) == false)
                _abilityActions.Add(clonedAction.GetType(), clonedAction);
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
            int layer = ability.Definition.AnimationLayer;
            if(ability.Definition.AvatarMask) _animancerComponent.Layers[layer].SetMask(ability.Definition.AvatarMask);

            ability.PreviousAnimancerState = _animancerComponent.Layers[layer].CurrentState;
            _currentAbilityAnimState = _animancerComponent.Layers[layer].Play(ability.Definition.ClipTransition);
            ability.AnimancerState = _currentAbilityAnimState;
            
            /*Action onEndAction = () => EndOrInterrupted(_currentAbilityAnimState, ability);
            _onEndActions[_currentAbilityAnimState] = onEndAction;
            _currentAbilityAnimState.Events.OnEnd += onEndAction;*/
            
            _currentAbilityAnimationReachedFullWeight = false;
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

    public void EndOrInterrupted(AnimancerState animState, ActiveAbility activeAbility)
    {
        Debug.Log($"anim state : {animState.Clip.name} : {activeAbility}");
        if (animState == null || activeAbility == null) return;
        int layer = activeAbility.Definition.AnimationLayer;
        if (layer > 0)
        {
            _animancerComponent.Layers[layer].StartFade(0, activeAbility.Definition.ClipTransition.FadeDuration);
        }
            
        //animState.Events.OnEnd -= onEndAction;
 
        _onEndActions.Remove(animState);
        OnAnimEnd(activeAbility);
        if (_onEndActions.TryGetValue(animState, out System.Action onEndAction))
        {
            
        }
    }

    
    [Button]
    public void CancelCurrent()
    {
        _isLooping = false;
        OnAnimEnd(_abilityController.LastUsedAbility);
    }

    private void OnAnimEnd(ActiveAbility ability) // the animation has came to an end
    {
        if (_isLooping)
        {
            _animancerComponent.States.Current.Time = 0;
            //return;
        }

        _abilityAnimPlaying = false;
        if (ability is ActiveAbility activeAbility) // possible bug
        {
            _owner.GameplayTags.RemoveTags(activeAbility.Definition.AbilitySlotTags);
            _owner.GameplayTags.RemoveTags(activeAbility.Definition.GrantedTagsDuringAbility);
        }

        foreach (AbilityAction abilityAction in ability.Definition.AbilityActions) // exit lifetime actions
        {
            if (_abilityActions.TryGetValue(abilityAction.GetType(), out AbilityAction clonedAction))
            {
                if (abilityAction.ActivationPolicy == AbilityAction.EAbilityActionActivationPolicy.Lifetime)
                {
                    clonedAction.OnExit();
                    _abilityActions.Remove(clonedAction.GetType());
                    _registeredAbilityActionList.Remove(clonedAction);
                }
            }
        }

        _registeredAbilityActionList.Clear();
        _abilityActions.Clear();
        _abilityController.AbilityDoneAnimating(ability);

        _currentActiveAbility = null;
        _currentAbilityAnimState = null;
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