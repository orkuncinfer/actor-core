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
                EndOrInterrupted(_currentAbilityAnimState, ability);
            }
            
            foreach (var action in ability.AbilityActions)
            {
                if (action.ActivationPolicy != AbilityAction.EAbilityActionActivationPolicy.AnimWindow) continue;
                if (action.AnimWindow.x <= action.ActiveAbility.AnimancerState.NormalizedTime)
                {
                    if (!action.IsRunning &&
                        !action.HasExecutedOnce)
                    {
                        action.OnStart(_owner, ability);
                    }
                }
                if (action.AnimWindow.y <= action.ActiveAbility.AnimancerState.NormalizedTime && action.IsRunning)
                {
                    action.OnExit();
                }
                if (action.HasTick && action.IsRunning)
                    action.OnTick(_owner);
            }
        }

        if (_abilityAnimPlaying)
        {
            if (_currentActiveAbility != null)
            {
                
            }
        }
    }

    private void OnCanceledAbility(ActiveAbility ability)
    {
        foreach (AbilityAction abilityAction in ability.AbilityActions) // exit lifetime actions
        {
            if (abilityAction.ActivationPolicy == AbilityAction.EAbilityActionActivationPolicy.Lifetime)
            {
                abilityAction.OnExit();
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
            clonedAction.ActiveAbility = ability;
            clonedAction.AnimWindow = actionsToClone.AnimWindow;
            clonedAction.EventName = actionsToClone.EventName;
            clonedAction.ActivationPolicy = actionsToClone.ActivationPolicy;
            
            if(ability.AbilityActions == null) ability.AbilityActions = new List<AbilityAction>();
            ability.AbilityActions.Add(clonedAction);
         
            if (actionsToClone.ActivationPolicy == AbilityAction.EAbilityActionActivationPolicy.Lifetime)
            {
                clonedAction.OnStart(_owner, ability);
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
        //Debug.Log($"anim state : {animState.Clip.name} : {activeAbility}");
        if (animState == null || activeAbility == null) return;
        int layer = activeAbility.Definition.AnimationLayer;
        if (layer > 0)
        {
            _animancerComponent.Layers[layer].StartFade(0, activeAbility.Definition.ClipTransition.FadeDuration);
        }
 
        _onEndActions.Remove(animState);
        foreach (AbilityAction action in activeAbility.AbilityActions)
        {
            if (action.IsRunning)
            {
                action.OnExit();
            }
        }
        OnAnimEnd(activeAbility);
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

        foreach (AbilityAction abilityAction in ability.AbilityActions) // exit lifetime actions
        {
            if (abilityAction.ActivationPolicy == AbilityAction.EAbilityActionActivationPolicy.Lifetime)
            {
                abilityAction.OnExit();
            }
        }
        
        //_abilityActions.Clear();
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
                foreach (AbilityAction abilityAction in _currentActiveAbility.AbilityActions)
                {
                    if (!abilityAction.UseAnimEvent) continue;
                    if (abilityAction.EventName.Equals(newEventName, StringComparison.OrdinalIgnoreCase))
                    {
                        abilityAction.OnStart(_owner, _currentActiveAbility);
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

                foreach (AbilityAction abilityAction in _currentActiveAbility.AbilityActions)
                {
                    if (!abilityAction.UseAnimEvent) continue;
                    if (abilityAction.EventName.Equals(newEventName, StringComparison.OrdinalIgnoreCase))
                    {
                        abilityAction.OnExit();
                    }
                }
            }
        }
    }
}