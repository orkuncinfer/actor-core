
using System;
using System.Collections.Generic;
using System.Linq;
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
    private TagController m_TagController;
    public ActiveAbility CurrentAbility;
    public GameObject Target;
    public event Action<ActiveAbility> onActivatedAbility;
    public event Action onCancelCurrentAbility;

    public AbilityDefinition TestAbility;
    public event Action onInitialized;
    
    protected virtual void Awake()
    {
        _effectController = GetComponent<GameplayEffectController>();
        m_TagController = GetComponent<TagController>();

       
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
        CurrentAbility = null;
    }
    
    public bool TryActivateAbility(string abilityName, GameObject target)
    {
        if (m_Abilities.TryGetValue(abilityName, out Ability ability))
        {
            if (ability is ActiveAbility activeAbility)
            {
                if (!CanActivateAbility(activeAbility))
                    return false;
                this.Target = target;
                CurrentAbility = activeAbility;
                CommitAbility(activeAbility);
                onActivatedAbility?.Invoke(activeAbility);
                
                foreach (GameplayTag tag in activeAbility.Definition.GrantedTagsDuringAbility)
                {
                    m_TagController.AddTag(tag);
                }
                DDebug.Log($"<color=cyan>Ability</color> activated : {abilityName}");
                return true;
            }
        }
        DDebug.Log($"Ability with name {abilityName} not found!");
        return false;
    }
    
    public bool CanActivateAbility(ActiveAbility ability)
    {
        foreach (GameplayTag gameplayTag in ability.Definition.GrantedTagsDuringAbility) // is busy using ability?
        {
            if (m_TagController.Matches(gameplayTag))
            {
                DDebug.Log($"{ability.Definition.name} blocked by {gameplayTag.FullTag}!");
                return false;
            }
        }
        
        if (ability.Definition.Cooldown != null) // is on cooldown?
        {
            if (ability.Definition.Cooldown.GrantedTags.Count > 0)
            {
                if (m_TagController.Matches(ability.Definition.Cooldown.GrantedTags.First()))
                {
                    DDebug.Log($"{ability.Definition.name} is on cooldown!");
                    return false;
                }
            } 
            
        }
            
        if (ability.Definition.Cost != null)
            return _effectController.CanApplyAttributeModifiers(ability.Definition.Cost);
           
        return true;
    }
    
    private void CommitAbility(ActiveAbility ability)
    {
         _effectController.ApplyGameplayEffectToSelf(new GameplayEffect(ability.Definition.Cost, ability, gameObject));
        _effectController.ApplyGameplayEffectToSelf(new GameplayPersistentEffect(ability.Definition.Cooldown, ability, gameObject));
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
        }
        IsInitialized = true;
        onInitialized?.Invoke();
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
                    abilities.Add(ability.AbilityDefinition.name,savable.data);
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