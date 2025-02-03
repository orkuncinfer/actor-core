using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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
    
    private InputAction _abilityAction;
    private bool _start;
    private bool _activatedOnce;
    protected override void OnEnter()
    {
        base.OnEnter();
        _gasData = Owner.GetData<Data_GAS>();
        _abilityDS.GetData(Owner);
        
        _abilityAction = ActionAsset.FindAction(ActionName);
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
        if (_start && TryActivateWhenHolding)
        {
            StartAbility();
        }else if (ActivateOnceWhenHolding && _start && !_activatedOnce)
        {
            StartAbility();
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
        _start = true;
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
}
