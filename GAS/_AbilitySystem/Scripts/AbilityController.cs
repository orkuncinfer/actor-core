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
    [ShowInInspector]protected Dictionary<string, Ability> m_Abilities = new Dictionary<string, Ability>();
    public Dictionary<string, Ability> Abilities => m_Abilities;

    private GameplayEffectController _effectController;
    public ActiveAbility LastUsedAbility;
    [ShowInInspector]private List<ActiveAbility> _activeAbilities = new List<ActiveAbility>();

    private List<GameplayTag> _queuedAbilityTags = new List<GameplayTag>();
    private ActiveAbilityDefinition _queuedAbilityToStart;

    [SerializeField] private GameObject _target;
    public GameObject Target
    {
        get => _target;
        set
        {
            GameObject oldValue = _target;
            _target = value;
            if (_target != oldValue)
            {
                OnTargetChanged?.Invoke(oldValue, _target);
            }
        }
    }

    public event Action<GameObject, GameObject> OnTargetChanged; 
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
        _owner.GameplayTags.OnTagChanged += OnTagChanged;
    }

    private void OnDestroy()
    {
        _owner.GameplayTags.OnTagChanged -= OnTagChanged;
    }

    private void OnTagChanged()
    {
        Debug.Log("Tag changed . Tried to activate on tag changed");
        TryActivateQueuedAbilities();
    }

    protected virtual void OnEnable()
    {
        _effectController.onInitialized += OnEffectControllerInitialized;
        if (_effectController.IsInitialized)
        {
            OnEffectControllerInitialized();
        }
    }
    
    public List<ActiveAbility> GetActiveAbilities()
    {
        return _activeAbilities;
    }

    public ActiveAbility TryActiveAbilityWithDefinition(AbilityDefinition definition)
    {
        //if (Target == null) Target = gameObject;
        if (TryActivateAbility(definition.name, Target))
        {
            return LastUsedAbility;
        }
        else
        {
           return null;
        }
    }

    public ActiveAbility TryActivateAbilityWithGameplayTag(GameplayTag tag)
    {
        foreach (var ability in m_Abilities)
        {
            if(ability.Value.AbilityDefinition is ActiveAbilityDefinition activeAbilityDefinition)
            {
                if (activeAbilityDefinition.AbilityTags.HasTag(tag))
                {
                    if (TryActivateAbility(ability.Key, Target))
                    {
                        return LastUsedAbility;
                    }
                }
            }
        }
        return null;
    }
    
    public ActiveAbility TryActivateSpecificAbilityWithGameplayTag(GameplayTag tag)
    {
        foreach (var ability in m_Abilities)
        {
            if(ability.Value.AbilityDefinition is ActiveAbilityDefinition activeAbilityDefinition)
            {
                if (activeAbilityDefinition.AbilityTags.HasTagExact(tag))
                {
                    if (TryActivateAbility(ability.Key, Target))
                    {
                        return LastUsedAbility;
                    }
                }
            }
        }
        return null;
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

    public ActiveAbility AddAndTryActivateAbility(AbilityDefinition definition)
    {
        AddAbilityIfNotHave(definition);
        if(TryActivateAbility(definition.name, Target))
        {
            return LastUsedAbility;
        }
        else
        {
            return null;
        }
    }

    public bool TryActivateAbility(string abilityName, GameObject target)
    {
        if (m_Abilities.TryGetValue(abilityName, out Ability ability))
        {
            if (ability is ActiveAbility activeAbility)
            {
                if (!CanActivateAbility(activeAbility)) return false;
                this.Target = target;
                _activeAbilities.Add(activeAbility);
                LastUsedAbility = activeAbility;
                ApplyAbilityEffects(activeAbility);
                CancelWithTagCheck(activeAbility);
                HandleAbilityLayer(activeAbility);
                activeAbility.StartAbility();
                activeAbility.onFinished += OnActivateAbilityFinished;
                onActivatedAbility?.Invoke(activeAbility);
                _queuedAbilityToStart = null;
                return true;
            }
        }
        DDebug.Log($"Ability with name {abilityName} not found!");
        return false;
    }

    private void OnActivateAbilityFinished(ActiveAbility obj)
    {
        obj.onFinished -= OnActivateAbilityFinished;
        
        TryActivateQueuedAbilities();
    }



    public void ForceActivateAbility(string abilityName)
    {
        if (m_Abilities.TryGetValue(abilityName, out Ability ability))
        {
            if (ability is ActiveAbility activeAbility)
            {
                _activeAbilities.Add(activeAbility);
                LastUsedAbility = activeAbility;
                ApplyAbilityEffects(activeAbility);
                CancelWithTagCheck(activeAbility);
                activeAbility.StartAbility();
                onActivatedAbility?.Invoke(activeAbility);
            }
        }
    }

    private void HandleAbilityLayer(ActiveAbility ability)
    {
        foreach (var activeAbility in _activeAbilities)
        {
            if (activeAbility.Definition.AbilityLayer == ability.Definition.AbilityLayer &&
                activeAbility.CanBeCanceled)
            {
                CancelAbilityIfActive(activeAbility);
                break;
            }
        }
    }
    public void ForceActivateAbilityWithTag(GameplayTag gameplayTag)
    {
        foreach (var ability in m_Abilities)
        {
            if (ability.Value.AbilityDefinition is ActiveAbilityDefinition activeAbilityDefinition)
            {
                if (activeAbilityDefinition.AbilityTags.HasTag(gameplayTag))
                {
                    ForceActivateAbility(activeAbilityDefinition.name);
                    return;
                }
            }
        }
        DDebug.Log($"No ability with tag {gameplayTag} found!");
    }

    private void CancelWithTagCheck(ActiveAbility activeAbility)
    {
        //Debug.Log($" CancelWithTagCheck {activeAbility.Definition.name} : tag count {activeAbility.Definition.CancelAbilitiesWithTag.TagCount}");
        if(activeAbility.Definition.CancelAbilitiesWithTag.TagCount == 0) return;
        for(int i = _activeAbilities.Count - 1; i >= 0; i--)
        {
            if (activeAbility.Definition.CancelAbilitiesWithTag.HasAny(_activeAbilities[i].Definition.AbilityTags))
            {
                Debug.Log($"Tried to cancel {_activeAbilities[i].Definition.name} because of tag {activeAbility.Definition.CancelAbilitiesWithTag.GetTags().First()}");
                CancelAbilityIfActive(_activeAbilities[i]);
            }
        }
    }

    public bool CanActivateAbility(AbilityDefinition definition)
    {
        if (m_Abilities.TryGetValue(definition.name, out Ability ability))
        {
            if (ability is ActiveAbility activeAbility)
            {
                if (!CanActivateAbility(activeAbility)) return false;
                return true;
            }
        }

        return false;
    }
    
    public bool CanActivateAbilityWithTag(GameplayTag gameplayTag, out ActiveAbilityDefinition abilityDefinition)
    {
        abilityDefinition = null;
        if (m_Abilities.Count == 0) return false;
        foreach (var ability in m_Abilities)
        {
            if (ability.Value.AbilityDefinition is ActiveAbilityDefinition activeAbilityDefinition)
            {
                if (activeAbilityDefinition.AbilityTags.HasTag(gameplayTag, out GameplayTag matchingTag))
                {
                    if (CanActivateAbility(activeAbilityDefinition))
                    {
                        abilityDefinition = activeAbilityDefinition;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool CanActivateAbility(GameplayTag gameplayTag)
    {
        foreach (var ability in m_Abilities)
        {
            if (ability.Value.AbilityDefinition is ActiveAbilityDefinition activeAbilityDefinition)
            {
                if (activeAbilityDefinition.AbilityTags.HasTag(gameplayTag))
                {
                    if (CanActivateAbility(activeAbilityDefinition))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private int iteration = 0;
    public bool CanActivateAbility(ActiveAbility ability)
    {
        iteration++;
        /* if (_owner.GameplayTags.HasTagExact("State.IsDead"))
         {
             DDebug.Log("Can't activate ability while dead!");
             return false;
         }
        
        if (_activeAbilities.Contains(ability))
        {
            return false; // already using an instance of this ability
        }*/
        if (ability.Definition.Cooldown != null) // is on cooldown?
        {
            if (ability.Definition.Cooldown.GrantedTags.TagCount > 0)
            {
                if (_owner.GameplayTags.HasTag(ability.Definition.Cooldown.GrantedTags.GetTags().First()))
                {
                    DDebug.Log($"{ability.Definition.name} is on cooldown!");
                    return false;
                }
            }
        }
        if (ability.Definition.Cost != null)
        {
            bool haveCost = _effectController.CanApplyAttributeModifiers(ability.Definition.Cost);
            if (!haveCost) return false;
        }

        foreach (var requiredTag in ability.Definition.ActivationRequiredTags.GetTags())
        {
            if(_owner.GameplayTags.HasTag(requiredTag) == false)
            {
                DDebug.Log($"Can't activate ability {ability.Definition.name} because required tag {requiredTag.FullTag} is missing {iteration}");
                return false;
            }
        }
        
        if (ability.Definition.ActivationBlockedTags.HasAnyExact(_owner.GameplayTags))
        {
            DDebug.Log($"Can't activate ability {ability.Definition.name} because blocked tag is present {iteration}");
            return false;
        }

        int abilityLayer = ability.Definition.AbilityLayer;
        bool abilityLayerIsBusy = false;

        foreach (ActiveAbility activeAbility in _activeAbilities)
        {
            if (activeAbility.Definition.name == ability.Definition.name)
            {
                DDebug.Log($"Can't activate ability {ability.Definition.name} because it is already active {iteration}");
                return false; // already using an instance of this ability
            }
        }

        foreach (ActiveAbility activeAbility in _activeAbilities)
        {
            if (activeAbility.AnimancerState == null) continue;
            if (activeAbility.Definition.AbilityLayer == abilityLayer)
            {
                Debug.Log("Canceling ability " + activeAbility.Definition.name +" to run ability :" + ability.Definition.name + "="+iteration);
                //CancelAbilityIfActive(activeAbility);
        
                if (activeAbility.CanBeCanceled == false)
                {
                    abilityLayerIsBusy = true;
                }
            }
        }
        if (abilityLayerIsBusy)
        {
            DDebug.Log("Ability layer is busy");
            return false;
        }
        return true;
    }

    private void ApplyAbilityEffects(ActiveAbility ability)
    {
        if (ability.Definition.Cost) _effectController.ApplyGameplayEffectDefinition(ability.Definition.Cost.ItemId,_owner.gameObject);
        if(ability.Definition.Cooldown)_effectController.ApplyGameplayEffectDefinition(ability.Definition.Cooldown.ItemId,_owner.gameObject);
    }

    public void AbilityDoneAnimating(ActiveAbility ability) // ability finished successfully at end of animation
    {
        if (_activeAbilities.Contains(ability))
        {
            _activeAbilities.Remove(ability);
            ability.EndAbility();
            Debug.Log("ability done animating");
        }
    }

    public void CancelAbilityIfActive(ActiveAbility ability)
    {
        string activeAbilityNames = "";
        foreach (var activeAbility in _activeAbilities)
        {
            activeAbilityNames += activeAbility.Definition.name + " ";
        }
        if (_activeAbilities.Contains(ability))
        {
            Debug.Log($"Requested cancelation of {ability.Definition.name} active abilities are {activeAbilityNames}");
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
    
    public void CancelAbilityWithGameplayTag(GameplayTag tag)
    {
        for(int i = _activeAbilities.Count - 1; i >= 0; i--)
        {
            if (_activeAbilities[i].Definition.AbilityTags.HasTag(tag))
            {
                CancelAbilityIfActive(_activeAbilities[i]);
            }
        }
    }

    public override void Initialize()
    {
        if(IsInitialized) return;
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
            if (ability is PassiveAbility passiveAbility)
            {
                passiveAbility.ApplyEffects(gameObject);
            }

        }
    }
    public void RemoveAbilityIfHave(AbilityDefinition abilityDefinition)
    {
        if (m_Abilities.ContainsKey(abilityDefinition.name))
        {
            m_Abilities[abilityDefinition.name] = null;
            m_Abilities.Remove(abilityDefinition.name);
        }
    }
    
    private void TryActivateQueuedAbilities()
    {
        if(_queuedAbilityToStart != null) return;
        //reverse for queued tags
        for (int i = _queuedAbilityTags.Count - 1; i >= 0; i--)
        {
            GameplayTag queuedTag = _queuedAbilityTags[i];
            if (CanActivateAbilityWithTag(queuedTag, out ActiveAbilityDefinition definition))
            {
                _queuedAbilityToStart = definition;
                Debug.Log("trying to activate ability : " + definition.name);
                TryActivateAbility(definition.name, _target);
                /*ActiveAbility ability = TryActivateAbilityWithGameplayTag(queuedTag);
                if (ability != null)
                {
                    //UnregisterQueueAbilityGameplayTag(queuedTag);
                }*/
            }
        }
    }

    public void RegisterQueueAbilityGameplayTag(GameplayTag gameplayTag)
    {
        if(!_queuedAbilityTags.Contains(gameplayTag))
            _queuedAbilityTags.Add(gameplayTag);
    }
    public void UnregisterQueueAbilityGameplayTag(GameplayTag gameplayTag)
    {
        if(_queuedAbilityTags.Contains(gameplayTag))
            _queuedAbilityTags.Remove(gameplayTag);
    }
    
    [Button]
    public void Test()
    {
        AddAndTryActivateAbility(TestAbility);
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