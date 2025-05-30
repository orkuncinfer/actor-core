using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using Sirenix.OdinInspector;
using StatSystem;
using UnityEngine;
using Attribute = StatSystem.Attribute;

public partial class GameplayEffectController :
#if USING_FISHNET
    NetworkBehaviour
#else
    MonoBehaviour
#endif
{
    protected StatController _statController;
    protected TagController _tagController;

    [SerializeField] private ItemListDefinition _allEffectsList;

    [SerializeField] private List<GameplayEffectDefinition> _initialEffects;

    public event Action onInitialized;
    public bool IsInitialized;


    [ShowInInspector] [ReadOnly]
    protected List<GameplayPersistentEffect> _activeEffects = new List<GameplayPersistentEffect>();

    public List<GameplayPersistentEffect> ActiveEffects => _activeEffects;

    [ShowInInspector] [ReadOnly]
    private List<GameplayEffectDefinition> _effectHistory = new List<GameplayEffectDefinition>();

    private List<GameplayPersistentEffect> _effectsToRemove = new List<GameplayPersistentEffect>();

    private ActorBase _owner;

    #region PUBLIC

    [Button]
    public void GetEffect()
    {
        DDebug.Log(_activeEffects.Count);
    }

    public bool ApplyGameplayEffectDefinition(string effectDefinitionId, bool asObserver = false) // source id?
    {
        object def = _allEffectsList.GetItem(effectDefinitionId);
        Debug.Log("found item " + def.GetType());
        GameplayEffectDefinition effectDefinition = def as GameplayEffectDefinition;
        Debug.Log("applying gameplay effect definition " + effectDefinition.name);
        EffectTypeAttribute attribute = effectDefinition.GetType().GetCustomAttributes(true)
            .OfType<EffectTypeAttribute>().FirstOrDefault();

        GameplayEffect effect = null;
        if (attribute.type == typeof(GameplayEffect))
        {
            effect = new GameplayEffect(effectDefinition, default, gameObject);
        }
        else
        {
            effect =
                Activator.CreateInstance(attribute.type, effectDefinition, this, gameObject) as
                    GameplayEffect;
        }

        return ApplyGameplayEffectToSelf(effect, asObserver);
    }

    public bool CanApplyAttributeModifiers(GameplayEffectDefinition effectDefinition)
    {
        foreach (var modifier in effectDefinition.ModifierDefinitions)
        {
            if (_statController.Stats.TryGetValue(modifier.StatName,
                    out Stat stat)) // if stat controller has a stat that we can modify
            {
                if (stat is Attribute attribute)
                {
                    if (modifier.Type == ModifierOperationType.Additive)
                    {
                        float cost = modifier.Formula.CalculateValue(gameObject);

                        if (attribute.CurrentValue < cost)
                        {
                            DDebug.Log(
                                $"{effectDefinition.name} cannot satisfy costs!  Cost: {cost}, Value: {attribute.CurrentValue}");
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

    #endregion PUBLIC

    #region MONOBEHAVIOUR

    private void Update()
    {
        HandleDuration();
    }

    private void OnEnable()
    {
        _statController.onInitialized += OnStatControllerInitialized;
        if (_statController.IsInitialized)
        {
            OnStatControllerInitialized();
        }

        _owner = ActorUtilities.FindFirstActorInParents(transform);
    }

    private void OnDisable()
    {
        _statController.onInitialized -= OnStatControllerInitialized;
    }

    private void Awake()
    {
        _statController = GetComponent<StatController>();
        _tagController = GetComponent<TagController>();
    }

    #endregion MONOBEHAVIOUR

#if USING_FISHNET
    public override void OnStartClient()
    {
        base.OnStartClient();
        
    }
#endif

    private bool
        ApplyGameplayEffectToSelf(GameplayEffect effectToApply,
            bool asObserver =
                false) // asObserver is needed for not looking for conditions because order given from server
    {
#if USING_FISHNET
        if (!base.IsServer && !asObserver) return false;
#endif

        bool isAdded = true;

        /*if (_owner.GameplayTags.HasTagExact("State.IsDead"))
        {
            DDebug.Log("Can't apply effect while dead!");
            return false;
        }*/

        foreach (GameplayPersistentEffect activeEffect in _activeEffects)
        {
            if (asObserver) continue;
            foreach (var tag in activeEffect.Definition.GrantedTags.GetTags())
            {
                if (effectToApply.Definition.ApplicationBlockerTags.GetTags()
                    .Any(t => t.FullTag == tag.FullTag)) //todo: check if it works
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
            GameplayStackableEffect existingStackableEffect =
                _activeEffects.Find(activeEffect => activeEffect.Definition == effectToApply.Definition) as
                    GameplayStackableEffect;

            if (existingStackableEffect != null)
            {
                isAdded = false;
                if (existingStackableEffect.StackCount ==
                    existingStackableEffect.Definition.StackLimitCount) //overflow exists
                {
                    foreach (GameplayEffectDefinition overflowEffectDef in existingStackableEffect.Definition
                                 .OverflowEffects)
                    {
                        EffectTypeAttribute attribute = overflowEffectDef.GetType().GetCustomAttributes(true)
                            .OfType<EffectTypeAttribute>().FirstOrDefault();

                        GameplayEffect overflowEffect = Activator.CreateInstance(attribute.type, overflowEffectDef,
                            existingStackableEffect, gameObject) as GameplayEffect;
                        ApplyGameplayEffectToSelf(overflowEffect);
                    }

                    if (existingStackableEffect.Definition.ClearStackOnOverflow) // sıkıntı var
                    {
                        RemoveActiveGameplayEffect(existingStackableEffect, true);
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

            GameplayEffect conditionalEffectInstance =
                Activator.CreateInstance(attribute.type, conditionalEffect, effectToApply, effectToApply.Instigator) as
                    GameplayEffect;
            ApplyGameplayEffectToSelf(conditionalEffectInstance);
        }

        List<GameplayPersistentEffect> effectsToRemove = new List<GameplayPersistentEffect>(); // check if it works!
        foreach (GameplayPersistentEffect activeEffect in _activeEffects)
        {
            foreach (var tag in activeEffect.Definition.GrantedTags.GetTags())
            {
                if (effectToApply.Definition.RemoveEffectsWithTags.GetTags()
                    .Any(t => t.FullTag == tag.FullTag)) //todo: check if it works
                {
                    effectsToRemove.Add(activeEffect);
                }
            }
        }

        foreach (var effect in effectsToRemove)
        {
            RemoveActiveGameplayEffect(effect, true);
        }

        if (effectToApply is GameplayPersistentEffect persistentEffect)
        {
            if (isAdded)
                AddPersistentGameplayEffect(persistentEffect);
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
#if USING_FISHNET
        if (base.IsServerOnly && !asObserver)
        {
            Observer_ApplyGameplayEffectsToSelf(base.Owner,effectToApply.Definition.ItemId);
        }
#endif
        return true;
    }

#if USING_FISHNET
    [ObserversRpc(BufferLast = true, ExcludeOwner = false)]
    private void Observer_ApplyGameplayEffectsToSelf(NetworkConnection conn, string effectDefinitionId)
    {
        Debug.Log("PLAYED AS TARGET RPC");
        ApplyGameplayEffectDefinition(effectDefinitionId, true);
    }
#endif


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

            if (!activeEffect.Definition.IsInfinite)
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
            RemoveActiveGameplayEffect(effect, false);
        }
    }

    private void RemoveActiveGameplayEffect(GameplayPersistentEffect effect, bool prematureRemoval)
    {
        RemoveUninhibitedEffects(effect.Definition.ItemId);
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
            RemoveActiveGameplayEffect(effectToRemove, true);
        }
    }

    private void AddPersistentGameplayEffect(GameplayPersistentEffect persistentEffect)
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
            if (_statController.Stats.TryGetValue(effect.Definition.ModifierDefinitions[i].StatName, out Stat stat))
            {
                if (stat is Attribute attr) // WHY ADDED ?
                {
                    if (effect.Modifiers[i] is HealthModifier)
                    {
                        continue;
                    }
                    attr.ApplyTempModifier(effect.Modifiers[i]);
                    Debug.Log("Applied temp mod to : " + attr.Definition.name);
                }
                else
                {
                    stat.AddModifier(effect.Modifiers[i]);
                }

                //stat.AddModifier(effect.Modifiers[i]);
            }
        }

        _owner.GameplayTags.AddTags(effect.Definition.GrantedTags);

        if (effect.Definition.SpecialPersistentEffectDefinition != null)
        {
            PlaySpecialEffectPersistent(effect.Definition.ItemId);
        }
    }

    private void RemoveUninhibitedEffects(string effectId, bool asObserver = false)
    {
#if USING_FISHNET
        if (!base.IsServer && !asObserver)
        {
            return;
        }
#endif

        GameplayPersistentEffect effectToRemove = null;

        foreach (var activeEffect in _activeEffects)
        {
            if (activeEffect.Definition.ItemId == effectId)
            {
                effectToRemove = activeEffect;
            }
        }

        if (effectToRemove == null) return;

        foreach (var modifierDefinition in effectToRemove.Definition.ModifierDefinitions)
        {
            if (_statController.Stats.TryGetValue(modifierDefinition.StatName, out Stat stat))
            {
#if USING_FISHNET
                    if(base.IsServer)
                        stat.RemoveModifierFromSource(effectToRemove);
#else
                stat.RemoveModifierFromSource(effectToRemove);
#endif
            }
        }

        _owner.GameplayTags.RemoveTags(effectToRemove.Definition.GrantedTags);

        if (effectToRemove.Definition.SpecialPersistentEffectDefinition != null)
        {
            StopSpecialEffectPersistent(effectToRemove.Definition.ItemId);
        }

        _activeEffects.Remove(effectToRemove);

#if USING_FISHNET
        if (base.IsServerOnly && !asObserver)
        {
            Observer_RemoveUninhibitedEffects(effectId);
        }
#endif
    }

#if USING_FISHNET
    [ObserversRpc(BufferLast = true,ExcludeOwner = false)]
    private void Observer_RemoveUninhibitedEffects(string effectId)
    {
        RemoveUninhibitedEffects(effectId, true);
    }
#endif

    private void ExecuteGameplayEffect(GameplayEffect effect)
    {
        ApplyEffectModifiers(effect);
    }

    void ApplyEffectModifiers(GameplayEffect effect)
    {
        for (int i = 0; i < effect.Modifiers.Count; i++)
        {
            Debug.Log(effect.Definition.name + effect.Definition.ModifierDefinitions.Count);
            if (_statController.Stats.TryGetValue(effect.Definition.ModifierDefinitions[i].StatName,
                    out Stat stat))
            {
                if (stat is Attribute attribute)
                {
#if USING_FISHNET
                     if(base.IsServer)
                        attribute.ApplyModifier(effect.Modifiers[i]); // only server can do this
#else
                    attribute.ApplyModifier(effect.Modifiers[i]);
#endif
                }
            }
        }
    }

    public void ApplyStatModifierExternal(StatModifier modifier, string statName) // not tested yet
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

    private void AddEffectToHistory(GameplayEffectDefinition effect)
    {
        _effectHistory.Insert(0, effect);
    }

    private void Initialize()
    {
        foreach (GameplayEffectDefinition effectDefinition in _initialEffects)
        {
            EffectTypeAttribute attribute = effectDefinition.GetType().GetCustomAttributes(true)
                .OfType<EffectTypeAttribute>().FirstOrDefault();

            GameplayEffect effect =
                Activator.CreateInstance(attribute.type, effectDefinition, _initialEffects, gameObject) as
                    GameplayEffect;
            ApplyGameplayEffectToSelf(effect);
        }

        IsInitialized = true;
        onInitialized?.Invoke();
    }
}