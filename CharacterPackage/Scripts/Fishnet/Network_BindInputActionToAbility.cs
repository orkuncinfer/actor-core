using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// Simplified network-aware input binding system with true input buffering.
/// Always extends input duration when abilities cannot activate immediately.
/// </summary>
public class Network_BindInputActionToAbility :
#if USING_FISHNET
    NetworkBehaviour
#else
    MonoBehaviour
#endif
{
    #region Configuration
    
    [Header("Ability Configuration")]
    [SerializeField] private bool _startWithTag;
    [SerializeField, ShowIf("_startWithTag")] private GameplayTag _tag;
    [FormerlySerializedAs("_abilityData"), SerializeField, HideIf("_startWithTag")]
    private DSGetter<Data_AbilityDefinition> _abilityDefinitionGetter;
    
    [Header("Input Configuration")]
    [SerializeField] private InputActionReference _actionReference;
    [SerializeField] private bool _cancelOnRelease;
    [SerializeField] private AbilityInputActivationPolicy _activationPolicy;
    
    [Header("Input Buffering")]
    [SerializeField, Range(0.1f, 3.0f)]
    [Tooltip("How long to keep trying activation after input")]
    private float _inputBufferDuration = 0.5f;
    
    [SerializeField, Range(0.05f, 0.5f)]
    [Tooltip("Minimum time between network requests")]
    private float _networkRequestCooldown = 0.1f;
    
    [Header("Ability Management")]
    [SerializeField] private GameplayTagContainer _cancelAbilitiesWithTag;
    
    [SerializeReference, TypeFilter("GetConditionTypes")]
    [ListDrawerSettings(ShowFoldout = true)]
    private List<StateCondition> _activationConditions = new List<StateCondition>();
    
    #endregion
    
    #region Private Fields
    
    private Actor _ownerActor;
    private Service_GAS _gasService;
    private AbilityDefinition _cachedAbilityDefinition;
    
    // Input State - Simplified to single timer-based system
    private bool _pressingInput;
    private bool _isInputActive;           // True when we should try activating
    private bool _hasActivatedThisInput;   // Prevent multiple activations per input
    private float _inputEndTime;           // When current input expires
    private float _lastNetworkRequestTime;
    
    #endregion
    
    #region Unity Lifecycle
    
#if USING_FISHNET
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        Initialize();
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner) SubscribeToInput();
    }
    
    public override void OnStopClient()
    {
        base.OnStopClient();
        UnsubscribeFromInput();
    }
#else
    private void Start()
    {
        Initialize();
        SubscribeToInput();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromInput();
    }
#endif
    
    private void Update()
    {
#if USING_FISHNET
        if (!IsOwner) return;
#endif
        ProcessInput();
    }
    
    #endregion
    
    #region Initialization
    
    private void Initialize()
    {
        _ownerActor = GetComponent<Actor>();
        if (_ownerActor == null)
        {
            Debug.LogError($"[{GetType().Name}] Actor component required!", this);
            return;
        }
        
        _gasService = _ownerActor.GetService<Service_GAS>();
        if (_gasService == null)
        {
            Debug.LogError($"[{GetType().Name}] GAS Service required!", this);
            return;
        }
        
        // Cache ability definition once
        if (!_startWithTag && _abilityDefinitionGetter != null)
        {
            _abilityDefinitionGetter.GetData(_ownerActor);
            _cachedAbilityDefinition = _abilityDefinitionGetter.Data?.AbilityDefinition;
        }
        
        // Initialize conditions
        foreach (var condition in _activationConditions)
        {
            condition.Initialize(_ownerActor);
        }
    }
    
    #endregion
    
    #region Input Handling
    
    private void SubscribeToInput()
    {
        if (_actionReference?.action == null) return;
        
        _actionReference.action.performed += OnInputTriggered;
        _actionReference.action.canceled += OnInputReleased;
        _actionReference.action.Enable();
    }
    
    private void UnsubscribeFromInput()
    {
        if (_actionReference?.action == null) return;
        
        _actionReference.action.performed -= OnInputTriggered;
        _actionReference.action.canceled -= OnInputReleased;
    }
    
    private void OnInputTriggered(InputAction.CallbackContext context)
    {
        // Start input session with buffering
        if (Time.time >= _inputEndTime) // no buffer exists
        {
#if USING_FISHNET
            ServerRegisterAbility(true);
#endif
            
        }
        _inputEndTime = Time.time + _inputBufferDuration;
        _pressingInput = true;
        _isInputActive = true;
        _hasActivatedThisInput = false;
        
        CancelConflictingAbilities();
    }
#if USING_FISHNET

    [ServerRpc]
    private void ServerRegisterAbility(bool register)
    {
        ObserverRegisterAbility(register);
        if(!IsServerOnly)return;
        if (register)
        {
            Debug.Log($"Server : Registered Ability {_tag.FullTag}");
            _gasService.AbilityController.TryActivateAbilityWithGameplayTag(_tag);
            _gasService.AbilityController.RegisterQueueAbilityGameplayTag(_tag);
        }
        else
        {
            Debug.Log($"Server : Unregistered Ability {_tag.FullTag}");
            _gasService.AbilityController.UnregisterQueueAbilityGameplayTag(_tag);
        }
        
    }
    [ObserversRpc(ExcludeOwner = false)]
    private void ObserverRegisterAbility(bool register)
    {
        if(IsServerOnly)return;
        if (register)
        {
            Debug.Log($"Server : Registered Ability {_tag.FullTag}");
            _gasService.AbilityController.TryActivateAbilityWithGameplayTag(_tag);
            _gasService.AbilityController.RegisterQueueAbilityGameplayTag(_tag);
        }
        else
        {
            Debug.Log($"Server : Unregistered Ability {_tag.FullTag}");
            _gasService.AbilityController.UnregisterQueueAbilityGameplayTag(_tag);
        }
    }
    
#endif
    private void OnInputReleased(InputAction.CallbackContext context)
    {
        _pressingInput = false;
        // For holding policies, reduce buffer time but don't stop immediately
        if (_activationPolicy == AbilityInputActivationPolicy.TryActivateWhenHolding)
        {
            _inputEndTime = Mathf.Min(_inputEndTime, Time.time + 0.1f); // Short grace period
        }
        
        // Handle immediate cancel on release
        if (_cancelOnRelease && !_hasActivatedThisInput)
        {
            CancelActiveAbility();
        }
    }
    
    private void ProcessInput()
    {
        if (!_isInputActive) return;

        if (_activationPolicy == AbilityInputActivationPolicy.TryActivateWhenHolding && _pressingInput)
        {
            _inputEndTime = Time.time + _inputBufferDuration; // Extend buffer while holding
        }
        
        // Check if input has expired
        if (Time.time >= _inputEndTime)
        {
            EndInput();
            return;
        }
        
        // Try activation based on policy
        bool shouldTryActivation = _activationPolicy switch
        {
            AbilityInputActivationPolicy.TryActivateWhenHolding => true,
            AbilityInputActivationPolicy.ActivateOnceWhenHolding => !_hasActivatedThisInput,
            _ => true
        };
   
        /*if (shouldTryActivation && TryActivateAbility())
        {
            // For performed policy, end buffering immediately on successful activation
            if (_activationPolicy == AbilityInputActivationPolicy.OnPerformed)
            {
                EndInput();
            }
        }*/
    }
    
    private void EndInput()
    {
        // Handle delayed cancel on release
        if (_cancelOnRelease && !_hasActivatedThisInput)
        {
            CancelActiveAbility();
        }
#if USING_FISHNET
        ServerRegisterAbility(false);
#endif
        _isInputActive = false;
        _hasActivatedThisInput = false;
    }
    
    #endregion
    
    #region Ability Activation
    
    private bool TryActivateAbility()
    {
        if (!CanActivateAbility()) return false;
        
        // Network cooldown check
        if (Time.time - _lastNetworkRequestTime < _networkRequestCooldown) return false;
        
        _lastNetworkRequestTime = Time.time;
        bool activationSuccessful = false;
        
#if USING_FISHNET
        if (IsOwner)
        {
            // Client prediction for immediate feedback
            if (!IsServerOnly && ActivateAbilityLocally())
            {
                _hasActivatedThisInput = true;
                activationSuccessful = true;
            }
            
            // Authoritative server request
            //ServerRpc_ActivateAbility();
        }
#else
        if (ActivateAbilityLocally())
        {
            _hasActivatedThisInput = true;
            activationSuccessful = true;
        }
#endif

        return activationSuccessful;
    }
    
    private bool CanActivateAbility()
    {
        if (_gasService?.AbilityController == null) return false;
        
        // Check custom conditions
        foreach (var condition in _activationConditions)
        {
            if (!condition.CheckCondition()) return false;
        }
        
        // Check ability availability
        if (_startWithTag)
        {
            return _gasService.AbilityController.CanActivateAbilityWithTag(_tag, out var definition);
        }
        
        return _cachedAbilityDefinition != null && 
               _gasService.AbilityController.CanActivateAbility(_cachedAbilityDefinition);
    }
    
    private bool ActivateAbilityLocally()
    {
        ActiveAbility result = null;
        
        if (_startWithTag)
        {
            result = _gasService.AbilityController.TryActivateAbilityWithGameplayTag(_tag);
        }
        else if (_cachedAbilityDefinition != null)
        {
            result = _gasService.AbilityController.TryActiveAbilityWithDefinition(_cachedAbilityDefinition);
        }
        
        return result != null;
    }
    
    #endregion
    
    #region Network RPCs
    
#if USING_FISHNET
    [ServerRpc]
    private void ServerRpc_ActivateAbility()
    {
        // Server validation and execution
        if (!CanActivateAbility()) return;
        return;
        if (IsServerOnly && ActivateAbilityLocally())
        {
            // Server successfully activated, inform observers
            ObserversRpc_ActivateAbility();
        }
    }
    
    [ObserversRpc]
    private void ObserversRpc_ActivateAbility()
    {
        // Skip owner and server (already executed)
        if (IsOwner || IsServer) return;
        
        // Force activation on observers for consistency
        if (_startWithTag)
        {
            _gasService?.AbilityController?.ForceActivateAbilityWithTag(_tag);
        }
        else if (_cachedAbilityDefinition != null)
        {
            _gasService?.AbilityController?.ForceActivateAbility(_cachedAbilityDefinition.name);
        }
    }
#endif
    
    #endregion
    
    #region Ability Management
    
    private void CancelConflictingAbilities()
    {
        if (_cancelAbilitiesWithTag?.GetTags() == null) return;
        
        foreach (var tag in _cancelAbilitiesWithTag.GetTags())
        {
            _gasService?.AbilityController?.CancelAbilityWithGameplayTag(tag);
        }
    }
    
    private void CancelActiveAbility()
    {
        if (_startWithTag)
        {
            _gasService?.AbilityController?.CancelAbilityWithGameplayTag(_tag);
        }
        else if (_cachedAbilityDefinition != null)
        {
            _gasService?.AbilityController?.CancelAbilityIfActive(_cachedAbilityDefinition.name);
        }
    }
    
    #endregion
    
    #region Public API
    
    public void SetInputBufferDuration(float duration) => 
        _inputBufferDuration = Mathf.Clamp(duration, 0.1f, 3.0f);
    
    public void SetNetworkRequestCooldown(float cooldown) => 
        _networkRequestCooldown = Mathf.Clamp(cooldown, 0.05f, 0.5f);
    
    public bool IsInputActive => _isInputActive;
    
    public float GetRemainingInputTime() => 
        _isInputActive ? Mathf.Max(0f, _inputEndTime - Time.time) : 0f;
    
    #endregion
    
    #region Utility
    
    public IEnumerable<Type> GetConditionTypes() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && 
                          !type.IsGenericTypeDefinition && 
                          typeof(StateCondition).IsAssignableFrom(type) && 
                          type != typeof(StateCondition));
    
    #endregion
    
    #region Debug
    
#if UNITY_EDITOR
    [Button("Debug State"), ShowInInspector]
    private void DebugState()
    {
        var remainingTime = GetRemainingInputTime();
        var networkCooldown = Mathf.Max(0f, _networkRequestCooldown - (Time.time - _lastNetworkRequestTime));
        
        Debug.Log($"[Input] Active: {_isInputActive} | " +
                  $"Remaining: {remainingTime:F2}s | " +
                  $"Activated: {_hasActivatedThisInput} | " +
                  $"Can Activate: {CanActivateAbility()} | " +
                  $"Network Cooldown: {networkCooldown:F2}s", this);
    }
#endif
    
    #endregion
}