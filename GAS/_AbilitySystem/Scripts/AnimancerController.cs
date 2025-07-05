using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
using DG.Tweening;
using FishNet.Managing.Timing;
using FishNet.Object;
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
    
    private List<AbilityAction> _actionsToProcess = new List<AbilityAction>();

    private Action _onAnimationEnd;
    
#if USING_FISHNET
    private NetworkObject _networkObject;
#endif

    private void Start()
    {
        _owner = ActorUtilities.FindFirstActorInParents(transform);
        _abilityController = _owner.GetService<Service_GAS>().AbilityController;
        _animancerComponent = GetComponent<AnimancerComponent>();

        _abilityController.onActivatedAbility += OnActivatedAbility;
        _abilityController.onCanceledAbility += OnCanceledAbility;
        
#if USING_FISHNET
        _networkObject = _owner.GetComponent<NetworkObject>();
        _networkObject.TimeManager.OnTick += OnTick;
#endif
    }

    private void OnDestroy()
    {
#if USING_FISHNET
        if(_networkObject != null)
            _networkObject.TimeManager.OnUpdate -= OnTick;
#endif
    }

#if USING_FISHNET
    private void OnTick()
    {
        ProcessAbilities();   
    }
#endif

#if !USING_FISHNET
    private void Update()
    {
        ProcessAbilities();
    }
#endif

    private void ProcessAbilities()
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
            if(ability.IsActive == false) continue; // in case it is canceled
            _actionsToProcess.Clear();
            _actionsToProcess.AddRange(ability.AbilityActions);

            if (ability.AnimancerState != null)// no animation ability
            {
                ability.NormTime = ability.AnimancerState.NormalizedTime;
                if (ability.AnimancerState.NormalizedTime >= ability.Definition.EndTime) // animation casually finished
                {
                    _isLooping = false;
                    Debug.Log("animation finished " + ability.AnimancerState.NormalizedTime + "-" + ability.Definition.EndTime);
                    EndOrInterrupted(ability.AnimancerState, ability);
                }
                if (ability.Definition.EndTime < 1) // ??
                { }
                
                if (ability.AnimancerState.EffectiveWeight > 0.9f && !ability.AnimationReachedFullWeight) // ANIMATION INTERRUPT CHECK
                {
                    ability.AnimationReachedFullWeight = true;
                }
                if (ability.AnimancerState.EffectiveWeight <= 0 && ability.AnimancerState.EffectiveWeight < ability.PreviousAnimWeight)
                {
                    EndOrInterrupted(_currentAbilityAnimState, ability);
                }
            } 

            
            foreach (var action in _actionsToProcess)
            {
                if (action.ActivationPolicy == AbilityAction.EAbilityActionActivationPolicy.Lifetime)
                {
                    if (action.HasTick && action.IsRunning)
                    {
                        action.OnTick(_owner);
                    }
                }
                if (action.ActivationPolicy != AbilityAction.EAbilityActionActivationPolicy.AnimWindow) continue;
                Debug.Log("action norm time : " + action.ActiveAbility.AnimancerState.NormalizedTime);
                if (action.AnimWindow.x <= action.ActiveAbility.AnimancerState.NormalizedTime)
                {
                    Debug.Log("Processing action : " + action.GetType() + $"isRunning={action.IsRunning} executed={action.HasExecutedOnce}");
                    if (!action.IsRunning &&
                        !action.HasExecutedOnce)
                    {
                        Debug.Log("Start action : " + action.GetType());
                        action.Owner = _owner;
                        action.ActiveAbility = ability;
                        action.OnStart();
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
        if (ability.AbilityActions == null) return;
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
            Debug.Log("Created added action :" + clonedAction.GetType());
         
            if (actionsToClone.ActivationPolicy == AbilityAction.EAbilityActionActivationPolicy.Lifetime)
            {
                clonedAction.Owner = _owner;
                clonedAction.ActiveAbility = ability;
                clonedAction.OnStart();
            }
        }

        if (ability.Definition.UseCustomAnim)
        {
            ability.onCustomClipTransitionSet += OnCustomAnimSet;
        }
        else
        {
            PlayClipTransition(ability);
        }
    }

    private void OnCustomAnimSet(ActiveAbility ability)
    {
        ability.onCustomClipTransitionSet -= OnCustomAnimSet;
        if(ability.IsActive)
            PlayClipTransition(ability,ability.CustomClipTransition);
    }

    public void PlayClipTransition(ActiveAbility ability, ClipTransition customAnim = null)
    {
        if (ability.Definition.AnimationClip || ability.Definition.UseCustomAnim)
        {
            int layer = ability.Definition.AnimationLayer;
            if(ability.Definition.AvatarMask) _animancerComponent.Layers[layer].SetMask(ability.Definition.AvatarMask);

            ClipTransition clip = ability.Definition.ClipTransition;
            if(customAnim != null) clip = customAnim;
            ability.PreviousAnimancerState = _animancerComponent.Layers[layer].CurrentState;
            
            float animSpeed = clip.Speed;
            if (ability.Definition.IsBasicAttack)
            {
                float attackSpeedStat = _abilityController.GetComponent<StatController>().Stats["AttackSpeed"].Value;
                animSpeed =
                    (attackSpeedStat / 100f) / (1 / ability.Definition.AnimationClip.length);
            }
            else
            {
                if (ability.Definition.OverrideAnimSpeed)
                {
                    animSpeed = ability.Definition.AnimationSpeed;
                }
            }

            float baseFadeDuration = clip.FadeDuration;
            float adjustedFadeDuration = CalculateAdjustedFadeDuration(baseFadeDuration, animSpeed);
            
            
            if (_currentAbilityAnimState != null && _animancerComponent.States.TryGet(clip,out AnimancerState statee))
            {
                statee.Time = 0;
                _currentAbilityAnimState = _animancerComponent.Layers[layer].Play(statee,adjustedFadeDuration);
            }
            else
            {
                _currentAbilityAnimState = _animancerComponent.Layers[layer].Play(clip);
            }
            ability.AnimancerState = _currentAbilityAnimState;
            ability.AnimancerState.Time = 0;
            ability.AnimancerState.NormalizedTime = 0;
            _currentAbilityAnimationReachedFullWeight = false;
            _abilityAnimPlaying = true;

            

            _animancerComponent.Layers[layer].CurrentState.Speed = animSpeed;
        }
        else if(ability.Definition.AnimationClip == null && ability.Definition.UseCustomAnim == false)
        {
            ReCast(ability);
        }
    }
    
    private float CalculateAdjustedFadeDuration(float baseFadeDuration, float animationSpeed)
    {
        // Prevent division by zero and handle edge cases
        if (animationSpeed <= 0f)
            return baseFadeDuration;

        // Use inverse relationship with smoothing curve
        // Formula: adjustedDuration = baseDuration / sqrt(speed)
        // This provides gentler scaling than pure inverse (baseDuration / speed)
        float speedFactor = Mathf.Sqrt(Mathf.Abs(animationSpeed));
        float adjustedDuration = baseFadeDuration / speedFactor;

        // Apply additional smoothing for extreme speeds
        if (animationSpeed > 3f) // Very fast animations
        {
            // Use logarithmic scaling for very high speeds to prevent overly short fades
            float logScaling = Mathf.Log10(animationSpeed) / Mathf.Log10(3f);
            adjustedDuration = baseFadeDuration / (1f + logScaling);
        }
        else if (animationSpeed < 0.5f) // Very slow animations
        {
            // Limit fade duration increase for very slow animations
            adjustedDuration = Mathf.Min(adjustedDuration, baseFadeDuration * 2f);
        }

        return adjustedDuration;
    }

    public void EndOrInterrupted(AnimancerState animState, ActiveAbility activeAbility)
    {
        //Debug.Log($"anim state : {animState.Clip.name} : {activeAbility}");
        if (animState == null || activeAbility == null) return;
        if(activeAbility.IsActive == false) return;
        
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
            _owner.GameplayTags.RemoveTags(activeAbility.Definition.GrantedTagsDuringAbility);
        }

        foreach (AbilityAction abilityAction in ability.AbilityActions) // exit lifetime actions
        {
            if (abilityAction.ActivationPolicy == AbilityAction.EAbilityActionActivationPolicy.Lifetime)
            {
                abilityAction.OnExit();
            }
        }
        
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
                        abilityAction.ActiveAbility = _currentActiveAbility;
                        abilityAction.Owner = _owner;
                        abilityAction.OnStart();
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