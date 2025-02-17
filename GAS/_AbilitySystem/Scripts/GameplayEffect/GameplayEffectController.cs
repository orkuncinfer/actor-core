using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StatSystem;
using UnityEngine;
using Attribute = StatSystem.Attribute;

public partial class GameplayEffectController : MonoInitializable
{
    protected StatController _statController;
    protected TagController _tagController;

    [SerializeField] private List<GameplayEffectDefinition> _initialEffects;

    public event Action onInitialized;
    

    [ShowInInspector][ReadOnly]protected List<GameplayPersistentEffect> _activeEffects = new List<GameplayPersistentEffect>();
    public List<GameplayPersistentEffect> ActiveEffects => _activeEffects;
    [ShowInInspector][ReadOnly] private List<GameplayEffectDefinition> _effectHistory = new List<GameplayEffectDefinition>();
    
    private List<GameplayPersistentEffect> _effectsToRemove = new List<GameplayPersistentEffect>();
    
    private ActorBase _owner;

    [Button]
    public void GetEffect()
    {
        DDebug.Log(_activeEffects.Count);
    }
    
    private void Update()
    {
        HandleDuration();
    }
    
    private void OnEnable()
    {
        _statController.onInitialized += OnStatControllerInitialized;
        if(_statController.IsInitialized) OnStatControllerInitialized();
        
        _owner = ActorUtilities.FindFirstActorInParents(transform);
    }

    private void OnDisable()
    {
        _statController.onInitialized -= OnStatControllerInitialized;
    }

    private void OnStatControllerInitialized()
    {
        Initialize();
    }
    

    private void HandleDuration()
    {
        _effectsToRemove.Clear();
        foreach (GameplayPersistentEffect activeEffect in _activeEffects)
        {
            if (activeEffect.Definition.IsPeriodic)
            {
                activeEffect.RemainingPeriod = Math.Max(activeEffect.RemainingPeriod - Time.deltaTime, 0f);

                if (Mathf.Approximately(activeEffect.RemainingPeriod, 0))
                {
                    ExecuteGameplayEffect(activeEffect);
                    activeEffect.RemainingPeriod = activeEffect.Definition.Period;
                }
            }
            if(!activeEffect.Definition.IsInfinite)
            {
                activeEffect.RemainingDuration = Math.Max(activeEffect.RemainingDuration - Time.deltaTime, 0f);
                if (Mathf.Approximately(activeEffect.RemainingDuration, 0f))
                {
                    if (activeEffect is GameplayStackableEffect stackableEffect)
                    {
                        switch (stackableEffect.Definition.StackingExpirationPolicy)
                        {
                            case GameplayEffectStackingExpirationPolicy.RemoveSingleStackAndRefreshDuration:
                                stackableEffect.StackCount--;
                                if (stackableEffect.StackCount == 0)
                                {
                                   _effectsToRemove.Add(stackableEffect);
                                }
                                else
                                {
                                    activeEffect.RemainingDuration = activeEffect.Duration;
                                }
                                break;
                            case GameplayEffectStackingExpirationPolicy.NeverRefresh:
                                _effectsToRemove.Add(stackableEffect);
                                break;
                        }
                    }
                    _effectsToRemove.Add(activeEffect);
                }
            }
        }

        foreach (GameplayPersistentEffect effect in _effectsToRemove)
        {   
            RemoveActiveGameplayEffect(effect,false);
        }
    }

    private void Awake()
    {
        _statController = GetComponent<StatController>();
        _tagController = GetComponent<TagController>();
    }

    public bool ApplyGameplayEffectToSelf(GameplayEffect effectToApply)
    {
        bool isAdded = true;

        /*if (_owner.GameplayTags.HasTagExact("State.IsDead"))
        {
            DDebug.Log("Can't apply effect while dead!");
            return false;
        }*/
        
        foreach (GameplayPersistentEffect activeEffect in _activeEffects)
        {
            foreach (var tag in activeEffect.Definition.GrantedTags.GetTags())
            {
                if (effectToApply.Definition.ApplicationBlockerTags.GetTags().Any(t => t.FullTag == tag.FullTag))  //todo: check if it works
                {
                    DDebug.Log($"Blocked by {tag.FullTag}");
                    return false;
                }
                {
                    DDebug.Log($"Immune to {effectToApply.Definition.name}");
                    return false;
                }
            }
        }
        
        if (effectToApply is GameplayStackableEffect stackableEffect)
        {
            GameplayStackableEffect existingStackableEffect = _activeEffects.Find(activeEffect => activeEffect.Definition == effectToApply.Definition) as GameplayStackableEffect;
            
            if (existingStackableEffect != null)
            {
                isAdded = false;
                if (existingStackableEffect.StackCount == existingStackableEffect.Definition.StackLimitCount)//overflow exists
                {
                    foreach (GameplayEffectDefinition overflowEffectDef in existingStackableEffect.Definition.OverflowEffects)
                    {
                        EffectTypeAttribute attribute = overflowEffectDef.GetType().GetCustomAttributes(true)
                            .OfType<EffectTypeAttribute>().FirstOrDefault();
                        
                        GameplayEffect overflowEffect = Activator.CreateInstance(attribute.type,overflowEffectDef, existingStackableEffect,gameObject) as GameplayEffect;
                        ApplyGameplayEffectToSelf(overflowEffect);
                    }
                    if (existingStackableEffect.Definition.ClearStackOnOverflow)// sıkıntı var
                    {
                        RemoveActiveGameplayEffect(existingStackableEffect,true);
                        isAdded = true;
                    }

                    if (existingStackableEffect.Definition.DenyOverflowApplication)
                    {
                        DDebug.Log("Denied overflow application");
                        return false;
                    }
                }

                if (!isAdded)
                {
                    existingStackableEffect.StackCount =
                        Math.Min(existingStackableEffect.StackCount + stackableEffect.StackCount,
                            existingStackableEffect.Definition.StackLimitCount);

                    if (existingStackableEffect.Definition.StackDurationRefreshPolicy ==
                        GameplayEffectStackingDurationPolicy.RefreshOnSuccessfulApplication)
                    {
                        existingStackableEffect.RemainingDuration = existingStackableEffect.Duration;
                    }

                    if (existingStackableEffect.Definition.StackPeriodResetPolicy ==
                        GameplayEffectStackingPeriodPolicy.ResetOnSuccessfulApplication)
                    {
                        existingStackableEffect.RemainingPeriod = existingStackableEffect.Definition.Period;
                    }
                    
                }
            }
        }

        foreach (GameplayEffectDefinition conditionalEffect in effectToApply.Definition.ConditionalEffects)
        {
            EffectTypeAttribute attribute = conditionalEffect.GetType().GetCustomAttributes(true)
                .OfType<EffectTypeAttribute>().FirstOrDefault();
            
            GameplayEffect conditionalEffectInstance = Activator.CreateInstance(attribute.type,conditionalEffect, effectToApply,effectToApply.Instigator) as GameplayEffect;
            ApplyGameplayEffectToSelf(conditionalEffectInstance);
        }

        List<GameplayPersistentEffect> effectsToRemove = new List<GameplayPersistentEffect>();// check if it works!
        foreach (GameplayPersistentEffect activeEffect in _activeEffects)
        {
            foreach (var tag in activeEffect.Definition.GrantedTags.GetTags())
            {
                if (effectToApply.Definition.RemoveEffectsWithTags.GetTags().Any(t => t.FullTag == tag.FullTag)) //todo: check if it works
                {
                    effectsToRemove.Add(activeEffect);
                }
            }
        }

        foreach (var effect in effectsToRemove)
        {
            RemoveActiveGameplayEffect(effect,true);
        }
        
        if (effectToApply is GameplayPersistentEffect persistentEffect)
        {
            if(isAdded)
                AddGameplayEffect(persistentEffect);
        }
        else
        {
            ExecuteGameplayEffect(effectToApply);
        }

        if (effectToApply.Definition.specialEffectDefinition != null)
        {
            PlaySpecialEffect(effectToApply);
        }
        AddEffectToHistory(effectToApply.Definition);
        return true;
    }

    private void RemoveActiveGameplayEffect(GameplayPersistentEffect effect, bool prematureRemoval)
    {
        _activeEffects.Remove(effect);
        RemoveUninhibitedEffects(effect);
    }

    public void RemoveEffectWithDefinition(GameplayEffectDefinition effectDefinition)
    {
        List<GameplayPersistentEffect> effectsToRemove = new List<GameplayPersistentEffect>();
        foreach (GameplayPersistentEffect effect in _activeEffects)
        {
            if (effect.Definition == effectDefinition)
            {
                effectsToRemove.Add(effect);
            }
        }
        foreach (var effectToRemove in effectsToRemove)
        {
            RemoveActiveGameplayEffect(effectToRemove,true);
        }
    }

    private void AddGameplayEffect(GameplayPersistentEffect persistentEffect)
    {
        _activeEffects.Add(persistentEffect);
        AddUninhibitedEffects(persistentEffect);

        if (persistentEffect.Definition.IsPeriodic)
        {
            if (persistentEffect.Definition.ExecutePeriodicEffectOnApplication)
            {
                ExecuteGameplayEffect(persistentEffect);
            }
        }
    }

    private void AddUninhibitedEffects(GameplayPersistentEffect effect)
    {
        for (int i = 0; i < effect.Modifiers.Count; i++)
        {
            if(_statController.Stats.TryGetValue(effect.Definition.ModifierDefinitions[i].StatName, out Stat stat))
            {
                if (stat is Attribute attr)
                {
                    attr.ApplyTempModifier(effect.Modifiers[i]);
                }
                else
                {
                    stat.AddModifier(effect.Modifiers[i]);
                }
            }
        }
        _owner.GameplayTags.AddTags(effect.Definition.GrantedTags);
        
        if (effect.Definition.SpecialPersistentEffectDefinition != null)
        {
            PlaySpecialEffect(effect);
        }
    }
    private void RemoveUninhibitedEffects(GameplayPersistentEffect effect)
    {
       
        foreach (var modifierDefinition in effect.Definition.ModifierDefinitions)
        {
            if(_statController.Stats.TryGetValue(modifierDefinition.StatName, out Stat stat))
            {
                stat.RemoveModifierFromSource(effect);
            }
        }
        
        _owner.GameplayTags.RemoveTags(effect.Definition.GrantedTags);

        if (effect.Definition.SpecialPersistentEffectDefinition != null)
        {
            StopSpecialEffect(effect);
        }
    }
    
    private void ExecuteGameplayEffect(GameplayEffect effect)
    {
       ApplyEffectModifiers(effect);
    }

    void ApplyEffectModifiers(GameplayEffect effect)
    {
        for (int i = 0; i < effect.Modifiers.Count; i++)
        {
            //Debug.Log(effect.Definition.name + effect.Definition.ModifierDefinitions.Count);
            if (_statController.Stats.TryGetValue(effect.Definition.ModifierDefinitions[i].StatName,
                    out Stat stat))
            {
                if (stat is Attribute attribute)
                {
                    attribute.ApplyModifier(effect.Modifiers[i]);
                }
            }
        }
    }

    public void ApplyStatModifierExternal(StatModifier modifier, string statName)// not tested yet
    {
        if (_statController.Stats.TryGetValue(statName,
                out Stat stat))
        {
            if (stat is Attribute attribute)
            {
                attribute.ApplyModifier(modifier);
            }
        }
    }

    public bool CanApplyAttributeModifiers(GameplayEffectDefinition effectDefinition)
    {
        foreach (var modifier in effectDefinition.ModifierDefinitions)
        {
            if (_statController.Stats.TryGetValue(modifier.StatName, out Stat stat)) // if stat controller has a stat that we can modify
            {
                if (stat is Attribute attribute)
                {
                    if (modifier.Type == ModifierOperationType.Additive)
                    {
                        
                        float cost = modifier.Formula.CalculateValue(gameObject);
                        
                        if (attribute.CurrentValue < cost)
                        {
                            DDebug.Log($"{effectDefinition.name} cannot satisfy costs!  Cost: {cost}, Value: {attribute.CurrentValue}");
                            return false;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Only addition is supported!");
                        return false;
                    }
                }
                else
                {
                    Debug.LogWarning($"{modifier.StatName} is not an attribute");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning($"{modifier.StatName} not found!");
                return false;
            }
        }

        return true;
    }

    private void AddEffectToHistory(GameplayEffectDefinition effect)
    {
        _effectHistory.Insert(0, effect);
    }

    public override void Initialize()
    {
        foreach (GameplayEffectDefinition effectDefinition in _initialEffects)
        {
            EffectTypeAttribute attribute = effectDefinition.GetType().GetCustomAttributes(true)
                .OfType<EffectTypeAttribute>().FirstOrDefault();
                
            GameplayEffect effect = Activator.CreateInstance(attribute.type, effectDefinition, _initialEffects, gameObject) as GameplayEffect;
            ApplyGameplayEffectToSelf(effect);
        }
        IsInitialized = true;
        onInitialized?.Invoke();
    }
}
