using System;
using System.Collections.Generic;
using System.Linq;
using BandoWare.GameplayTags;
using SaveSystem.Scripts.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class AbilityController : MonoInitializable, ISavable
{
    public List<AbilityDefinition> AbilityDefinitions;
    public AbilityDefinitionSetSO AbilitySet;
    protected Dictionary<string, Ability> m_Abilities = new Dictionary<string, Ability>();
    public Dictionary<string, Ability> Abilities => m_Abilities;

    private GameplayEffectController _effectController;
    public ActiveAbility LastUsedAbility;
    private List<ActiveAbility> _activeAbilities = new List<ActiveAbility>();

    public GameObject Target;
    public event Action<ActiveAbility> onActivatedAbility;
    public event Action<ActiveAbility> onCanceledAbility;
    public event Action onCancelCurrentAbility;

    public AbilityDefinition TestAbility;

    private ActorBase _owner;

    public event Action onInitialized;

    protected virtual void Awake()
    {
        _effectController = GetComponent<GameplayEffectController>();
        _owner = ActorUtilities.FindFirstActorInParents(transform);
    }

    protected virtual void OnEnable()
    {
        _effectController.onInitialized += OnEffectControllerInitialized;
        if (_effectController.IsInitialized)
        {
            OnEffectControllerInitialized();
        }
    }

    [Button]
    public void TryActiveAbilityWithDefinition(AbilityDefinition definition)
    {
        if (Target == null) Target = gameObject;
        TryActivateAbility(definition.name, Target);
    }
    
    public List<ActiveAbility> GetActiveAbilities()
    {
        return _activeAbilities;
    }

    public void TryActiveAbilityWithDefinition(AbilityDefinition definition, out ActiveAbility outAbility)
    {
        if (Target == null) Target = gameObject;
        if (TryActivateAbility(definition.name, Target))
        {
            outAbility = LastUsedAbility;
        }
        else
        {
            outAbility = null;
        }
    }


    protected virtual void OnDisable()
    {
        _effectController.onInitialized -= OnEffectControllerInitialized;
    }

    private void OnEffectControllerInitialized()
    {
        Initialize();
    }

    public void CancelCurrentAbility()
    {
        onCancelCurrentAbility?.Invoke();
        LastUsedAbility = null;
    }

    public void AddAndTryActivateAbility(AbilityDefinition definition)
    {
        AddAbilityIfNotHave(definition);
        TryActivateAbility(definition.name, Target);
    }

    public bool TryActivateAbility(string abilityName, GameObject target)
    {
        Debug.Log("try activate");
        if (m_Abilities.TryGetValue(abilityName, out Ability ability))
        {
            if (ability is ActiveAbility activeAbility)
            {
                Debug.Log("try activate1");
                if (!CanActivateAbility(activeAbility)) return false;
                this.Target = target;
                Debug.Log("try activate2");
                LastUsedAbility = activeAbility;
                ApplyAbilityEffects(activeAbility);
                onActivatedAbility?.Invoke(activeAbility);
                _activeAbilities.Add(activeAbility);
                activeAbility.StartAbility();
                return true;
            }
        }

        DDebug.Log($"Ability with name {abilityName} not found!");
        return false;
    }

    public bool CanActivateAbility(ActiveAbility ability)
    {
        /* if (_owner.GameplayTags.HasTagExact("State.IsDead"))
         {
             DDebug.Log("Can't activate ability while dead!");
             return false;
         }
        
        if (_activeAbilities.Contains(ability))
        {
            return false; // already using an instance of this ability
        }*/
        Debug.Log("canactivate0");
        if (ability.Definition.Cooldown != null) // is on cooldown?
        {
            if (ability.Definition.Cooldown.GrantedTags.TagCount > 0)
            {
                if (_owner.GameplayTags.HasTag(ability.Definition.Cooldown.GrantedTags.First()))
                {
                    DDebug.Log($"{ability.Definition.name} is on cooldown!");
                    return false;
                }
            }
        }
        Debug.Log("canactivate2");
        if (ability.Definition.Cost != null)
        {
            bool haveCost = _effectController.CanApplyAttributeModifiers(ability.Definition.Cost);
            if (!haveCost) return false;
        }

        foreach (var requiredTag in ability.Definition.ActivationRequiredTags)
        {
            if(_owner.GameplayTags.HasTag(requiredTag) == false)
            {
                DDebug.Log($"Can't activate ability {ability.Definition.name} because required tag {requiredTag} is missing");
                return false;
            }
        }
        

        int abilityLayer = ability.Definition.AbilityLayer;
        bool abilityLayerIsBusy = false;
        Debug.Log("canactivate1");
        foreach (ActiveAbility activeAbility in _activeAbilities)
        {
            if(activeAbility.AnimancerState == null) continue;
            if (activeAbility.Definition.AbilityLayer == abilityLayer)
            {
                if (activeAbility.CanBeCanceled == false)
                {
                    abilityLayerIsBusy = true;
                }
                else
                {
                    Debug.Log("return truee1");
                    CancelAbilityIfActive(ability);
                    //return true;
                }
            }
        }
        Debug.Log("ability layer busY? " + abilityLayerIsBusy);
        if (abilityLayerIsBusy)
        {
            DDebug.Log("Ability layer is busy");
            return false;
        }
        Debug.Log("return truee");
        return true;
    }

    private void ApplyAbilityEffects(ActiveAbility ability)
    {
        _effectController.ApplyGameplayEffectToSelf(new GameplayEffect(ability.Definition.Cost, ability, gameObject));
        _effectController.ApplyGameplayEffectToSelf(new GameplayPersistentEffect(ability.Definition.Cooldown, ability,
            gameObject));
    }

    public void AbilityDoneAnimating(ActiveAbility ability) // ability finished successfully at end of animation
    {
        if (_activeAbilities.Contains(ability))
        {
            _activeAbilities.Remove(ability);
            ability.EndAbility();
        }
    }

    public void CancelAbilityIfActive(ActiveAbility ability)
    {
        if (_activeAbilities.Contains(ability))
        {
            _activeAbilities.Remove(ability);
            ability.EndAbility();
            onCanceledAbility?.Invoke(ability);
        }
    }

    public void CancelAbilityIfActive(string abilityName)
    {
        if (m_Abilities.TryGetValue(abilityName, out Ability ability))
        {
            if (ability is ActiveAbility activeAbility)
            {
                CancelAbilityIfActive(activeAbility);
            }
        }
    }

    public override void Initialize()
    {
        foreach (AbilityDefinition abilityDefinition in AbilityDefinitions)
        {
            AbilityTypeAttribute abilityTypeAttribute = abilityDefinition.GetType().GetCustomAttributes(true)
                .OfType<AbilityTypeAttribute>().FirstOrDefault();
            Ability ability =
                Activator.CreateInstance(abilityTypeAttribute.type, abilityDefinition, this) as Ability;
            m_Abilities.Add(abilityDefinition.name, ability);
            if (ability is PassiveAbility passiveAbility)
            {
                passiveAbility.ApplyEffects(gameObject);
            }

            ability.Owner = _owner as Actor;
        }

        IsInitialized = true;
        onInitialized?.Invoke();
    }

    public void AddAbilityIfNotHave(AbilityDefinition abilityDefinition)
    {
        if (!m_Abilities.ContainsKey(abilityDefinition.name))
        {
            AbilityTypeAttribute abilityTypeAttribute = abilityDefinition.GetType().GetCustomAttributes(true)
                .OfType<AbilityTypeAttribute>().FirstOrDefault();
            Ability ability =
                Activator.CreateInstance(abilityTypeAttribute.type, abilityDefinition, this) as Ability;
            ability.Owner = _owner as Actor;
            m_Abilities.Add(abilityDefinition.name, ability);
        }
    }

    public virtual object data
    {
        get
        {
            Dictionary<string, object> abilities = new Dictionary<string, object>();

            foreach (Ability ability in m_Abilities.Values)
            {
                if (ability is ISavable savable)
                {
                    abilities.Add(ability.AbilityDefinition.name, savable.data);
                }
            }

            return new AbilityControllerData
            {
                Abilities = abilities
            };
        }
    }

    public virtual void Load(object data)
    {
        AbilityControllerData abilityControllerData = (AbilityControllerData)data;
        foreach (Ability ability in m_Abilities.Values)
        {
            if (ability is ISavable savable)
            {
                savable.Load(abilityControllerData.Abilities[ability.AbilityDefinition.name]);
            }
        }
    }

    [Serializable]
    protected class AbilityControllerData
    {
        public Dictionary<string, object> Abilities;
    }
}