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

    [SerializeField] [ShowIf("_startWithTag")]
    private GameplayTag _tag;

    [FormerlySerializedAs("_abilityData")] [SerializeField] [HideIf("_startWithTag")]
    private DSGetter<Data_AbilityDefinition> _abilityDS;

    public InputActionReference ActionReference;
    public bool CancelOnRelease;

    public AbilityInputActivationPolicy ActivationPolicy;

    public GameplayTagContainer CancelAbilitiesWithTag;

    [SerializeReference] [TypeFilter("GetConditionTypeList")] [ListDrawerSettings(ShowFoldout = true)]
    public List<StateCondition> Conditions = new List<StateCondition>();

    private Service_GAS _gasService;
    private InputAction _abilityAction;
    private bool _start;
    private bool _activatedOnce;

    protected override void OnEnter()
    {
        base.OnEnter();
        _gasService = Owner.GetService<Service_GAS>();
        _abilityDS.GetData(Owner);

        Conditions.ForEach(x => x.Initialize(Owner));


        ActionReference.action.performed += OnPerformed;
        ActionReference.action.canceled += OnCanceled;
        ActionReference.action.Enable();

        _activatedOnce = false;
    }

    protected override void OnExit()
    {
        base.OnExit();
        ActionReference.action.performed -= OnPerformed;
        ActionReference.action.canceled -= OnCanceled;
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
    }

    private void OnCanceled(InputAction.CallbackContext obj)
    {
        _start = false;
        _activatedOnce = false;
        if (CancelOnRelease)
        {
            if (_startWithTag)
            {
                _gasService.AbilityController.CancelAbilityWithGameplayTag(_tag);
            }
            else
            {
                _gasService.AbilityController.CancelAbilityIfActive(_abilityDS.Data.AbilityDefinition.name);
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
            _gasService.AbilityController.CancelAbilityWithGameplayTag(tag);
        }

        StartAbility();
    }

    private void StartAbility()
    {
        ActiveAbility ability = null;
        if (_startWithTag)
        {
            ability = _gasService.AbilityController.TryActivateAbilityWithGameplayTag(_tag);
        }
        else
        {
            ability = _gasService.AbilityController.TryActiveAbilityWithDefinition(_abilityDS.Data.AbilityDefinition);
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