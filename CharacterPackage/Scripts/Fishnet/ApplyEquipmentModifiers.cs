using System;
using FishNet.Object;
using StatSystem;
using UnityEngine;

public class ApplyEquipmentModifiers : MonoBehaviour
{
    [SerializeField] private ItemListDefinition _allEffects;
    [SerializeField] private GameplayTag _isServerTag;
    
    private Equippable _equippable;
    private bool _isModifiersApplied;
    private bool _isInitialized;

    private void Awake()
    {
        _equippable = GetComponent<Equippable>();
        
        if (_equippable == null)
        {
            Debug.LogError($"ApplyEquipmentModifiers requires Equippable component on {gameObject.name}");
            enabled = false;
            return;
        }

        if (_allEffects == null)
        {
            Debug.LogError($"_allEffects is not assigned on {gameObject.name}");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (!enabled) return;
        
        InitializeEquipmentHandlers();
    }

    private void OnEnable()
    {
        if (_isInitialized && _equippable != null && _equippable.IsEquipped && _equippable.Owner != null)
        {
            HandleEquipped(_equippable.Owner);
        }
    }

    private void OnDisable()
    {
        if (_equippable != null && _equippable.Owner != null && _isModifiersApplied)
        {
            HandleUnequipped(_equippable.Owner);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeEquipmentHandlers()
    {
        _equippable.onEquipped += HandleEquipped;
        _equippable.onUnequipped += HandleUnequipped;
        _isInitialized = true;

        if (_equippable.IsEquipped && _equippable.Owner != null)
        {
            HandleEquipped(_equippable.Owner);
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (_equippable != null)
        {
            _equippable.onEquipped -= HandleEquipped;
            _equippable.onUnequipped -= HandleUnequipped;
        }
    }

    private void HandleEquipped(ActorBase owner)
    {
        if (!IsValidForModification(owner) || _isModifiersApplied)
            return;

        ApplyStatModifiers(owner);
    }

    private void HandleUnequipped(ActorBase owner)
    {
        if (!IsValidForModification(owner) || !_isModifiersApplied)
            return;

        RemoveStatModifiers(owner);
    }

    private bool IsValidForModification(ActorBase owner)
    {
        if (owner == null)
            return false;

#if USING_FISHNET
        if (!owner.GameplayTags.HasTagExact(_isServerTag))
            return false;
#endif

        return true;
    }

    private void ApplyStatModifiers(ActorBase owner)
    {
        var gasService = owner.GetService<Service_GAS>();
        if (gasService?.StatController == null)
        {
            Debug.LogError($"Failed to get StatController from {owner.name}");
            return;
        }

        var statController = gasService.StatController;

        if (_equippable.ItemData?.Attributes == null)
        {
            Debug.LogWarning($"ItemData or Attributes is null on {gameObject.name}");
            return;
        }

        foreach (var itemDataAttribute in _equippable.ItemData.Attributes)
        {
            if (!TryApplyModifier(statController, itemDataAttribute))
            {
                Debug.LogError($"Failed to apply modifier for attribute {itemDataAttribute.Key} on {gameObject.name}");
                RemoveStatModifiers(owner);
                return;
            }
        }

        _isModifiersApplied = true;
    }

    private bool TryApplyModifier(StatController statController, System.Collections.Generic.KeyValuePair<string, string> itemDataAttribute)
    {
        var effectDefinition = _allEffects.GetItem(itemDataAttribute.Key) as GameplayEffectDefinition;
        if (effectDefinition?.ModifierDefinitions == null || effectDefinition.ModifierDefinitions.Count == 0)
        {
            Debug.LogError($"Invalid effect definition for {itemDataAttribute.Key}");
            return false;
        }

        if (!float.TryParse(itemDataAttribute.Value, out float magnitude))
        {
            Debug.LogError($"Failed to parse magnitude value: {itemDataAttribute.Value}");
            return false;
        }

        var modifierDefinition = effectDefinition.ModifierDefinitions[0];
        var targetStat = statController.GetStat(modifierDefinition.StatName);
        
        if (targetStat == null)
        {
            Debug.LogError($"Stat {modifierDefinition.StatName} not found");
            return false;
        }

        var newModifier = new StatModifier
        {
            Type = modifierDefinition.Type,
            Magnitude = magnitude,
            Source = this
        };

        targetStat.AddModifier(newModifier);
        return true;
    }

    private void RemoveStatModifiers(ActorBase owner)
    {
        var gasService = owner.GetService<Service_GAS>();
        if (gasService?.StatController == null)
            return;

        var statController = gasService.StatController;

        if (_equippable.ItemData?.Attributes == null)
            return;

        foreach (var itemDataAttribute in _equippable.ItemData.Attributes)
        {
            var effectDefinition = _allEffects.GetItem(itemDataAttribute.Key) as GameplayEffectDefinition;
            if (effectDefinition?.ModifierDefinitions == null || effectDefinition.ModifierDefinitions.Count == 0)
                continue;

            var targetStat = statController.GetStat(effectDefinition.ModifierDefinitions[0].StatName);
            targetStat?.RemoveModifierFromSource(this);
        }

        _isModifiersApplied = false;
    }
}