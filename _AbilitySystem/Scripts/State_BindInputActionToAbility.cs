using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public enum AbilityInputActivationPolicy
{
    OnPerformed = 0,
    TryActivateWhenHolding = 1,
    ActivateOnceWhenHolding = 2,
}
public class State_BindInputActionToAbility : MonoState
{
    [SerializeField] private bool _startWithTag;
    [SerializeField][ShowIf("_startWithTag")] private GameplayTag _tag;
    [FormerlySerializedAs("_abilityData")] [SerializeField][HideIf("_startWithTag")] private DSGetter<Data_AbilityDefinition> _abilityDS;
    [SerializeField] private Data_GAS _gasData;
    public InputActionAsset ActionAsset;
    public string ActionName;
    public bool CancelOnRelease;
    public bool TryActivateWhenHolding;
    public bool ActivateOnceWhenHolding;

    public AbilityInputActivationPolicy ActivationPolicy;
    
    public GameplayTagContainer CancelAbilitiesWithTag;
    
    [SerializeReference][TypeFilter("GetConditionTypeList")] [ListDrawerSettings(ShowFoldout = true)]
    public List<StateCondition> Conditions = new List<StateCondition>();
    
    private InputAction _abilityAction;
    private bool _start;
    private bool _activatedOnce;
    protected override void OnEnter()
    {
        base.OnEnter();
        _gasData = Owner.GetData<Data_GAS>();
        _abilityDS.GetData(Owner);
        
        _abilityAction = ActionAsset.FindAction(ActionName);
        
        Conditions.ForEach(x => x.Initialize(Owner));
        
        _abilityAction.performed += OnPerformed;
        _abilityAction.canceled += OnCanceled;
        
        _abilityAction?.Enable();

        _activatedOnce = false;
    }

    protected override void OnExit()
    {
        base.OnExit();
        _abilityAction.performed -= OnPerformed;
        _abilityAction.canceled -= OnCanceled;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        switch (ActivationPolicy)
        {
            case AbilityInputActivationPolicy.TryActivateWhenHolding:
                if (_start)
                {
                    StartAbility();
                }
                break;
            case AbilityInputActivationPolicy.ActivateOnceWhenHolding:
                if (_start && !_activatedOnce)
                {
                    StartAbility();
                }
                break;
        }
        /*
        if (_start && TryActivateWhenHolding)
        {
            StartAbility();
        }else if (ActivateOnceWhenHolding && _start && !_activatedOnce)
        {
            StartAbility();
        }*/
    }

    private void OnCanceled(InputAction.CallbackContext obj)
    {
        _start = false;
        _activatedOnce = false;
        if (CancelOnRelease)
        {
            if (_startWithTag)
            {
                _gasData.AbilityController.CancelAbilityWithGameplayTag(_tag);
            }
            else
            {
                _gasData.AbilityController.CancelAbilityIfActive(_abilityDS.Data.AbilityDefinition.name);
            }
        }
    }

    private void OnPerformed(InputAction.CallbackContext obj)
    {
        if (Conditions.Any(condition => condition.CheckCondition() == false))
        {
            return;
        }
        _start = true;
        foreach (var tag in CancelAbilitiesWithTag.GetTags())
        {
            _gasData.AbilityController.CancelAbilityWithGameplayTag(tag);
        }
        StartAbility();
    }

    private void StartAbility()
    {
        
        ActiveAbility ability = null;
        if (_startWithTag)
        {
            ability = _gasData.AbilityController.TryActivateAbilityWithGameplayTag(_tag);
        }
        else
        {
          ability = _gasData.AbilityController.TryActiveAbilityWithDefinition(_abilityDS.Data.AbilityDefinition);
        }

        if (ability != null)
        {
            _activatedOnce = true;
        }
    }
    
    public IEnumerable<Type> GetConditionTypeList()
    {
        var baseType = typeof(StateCondition);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var q = assemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsGenericTypeDefinition)
            .Where(x => baseType.IsAssignableFrom(x) && x != baseType); // Exclude the base class itself
        return q;
    }
}
