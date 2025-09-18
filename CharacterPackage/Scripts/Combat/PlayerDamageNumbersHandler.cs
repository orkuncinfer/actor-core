using System;
using System.Collections.Generic;
using DamageNumbersPro;
using StatSystem;
using UnityEngine;

public class PlayerDamageNumbersHandler : MonoBehaviour
{
    [SerializeField] private GameplayTag _playerTag;

    [SerializeField] private DamageNumber _numberPrefab;
    [SerializeField] private DamageNumber _criticalPrefab;
    [SerializeField] private DamageNumber _healPrefab;
    
    private Dictionary<int, Actor> _cachedActors = new Dictionary<int, Actor>();
    private void OnEnable()
    {
        GASEvents.OnModifierApplied += OnModifierApplied;
    }

    private void OnDisable()
    {
        GASEvents.OnModifierApplied -= OnModifierApplied;
    }

    private void HandleHealth(StatModifier modifier,Actor instigator, Actor target)
    {
        if (instigator.GameplayTags.HasTag(_playerTag))
        {
            if (target != instigator)
            {
                if (modifier.Magnitude < 0) // is a damage
                {
                    if (modifier is HealthModifier healthModifier)
                    {
                        float damage = Mathf.Abs(healthModifier.Magnitude);
                        DamageNumber damageNumber = _numberPrefab.Spawn(target.transform.position + new Vector3(0,1,0), damage,target.transform);
                        Debug.Log($"Player damaged {target.name} by {healthModifier.Magnitude}");
                    }
                }
            }
            else
            {
                if (modifier.Magnitude > 0) // is a heal to ourselves
                {
                    float heal = Mathf.Abs(modifier.Magnitude);
                    DamageNumber damageNumber = _healPrefab.Spawn(target.transform.position + new Vector3(0,1,0), heal,target.transform);
                    Debug.Log($"Player healed {target.name} by {heal}");
                }
            }
        }
    }

    private void OnModifierApplied(StatModifier modifier)
    {
        #region GetTargetSource
        GameObject target = modifier.Victim as GameObject;
        GameObject source = modifier.Instigator as GameObject;
        Debug.Log($"Modifier applied1 " + modifier.Source);
        
        if (target == null ||  source == null) return;
        
        Actor targetActor = null;
        Actor sourceActor = null;
        Debug.Log($"Modifier applied1 2 ");
        if (_cachedActors.ContainsKey(target.GetInstanceID()))
        {
            targetActor = _cachedActors[target.GetInstanceID()];
        }
        else
        {
            targetActor = target.GetComponent<Actor>();
            if (targetActor != null)
            {
                _cachedActors[target.GetInstanceID()] = targetActor;
            }
        }

        if (_cachedActors.ContainsKey(source.GetInstanceID()))
        {
            sourceActor = _cachedActors[source.GetInstanceID()];
        }
        else
        {
            sourceActor = source.GetComponent<Actor>();
            if (sourceActor != null)
            {
                _cachedActors[source.GetInstanceID()] = sourceActor;
            }
        }
        Debug.Log($"Modifier applied1 2  3{sourceActor.name} and {targetActor.name}");
        #endregion

        if (modifier.ModifierDefinition.StatName.Equals("Health",StringComparison.OrdinalIgnoreCase))
        {
            HandleHealth(modifier,sourceActor,targetActor);
        }
    }
}
