using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class State_BindInputActionToAbility : MonoState
{
    [FormerlySerializedAs("_abilityData")] [SerializeField] private DSGetter<Data_AbilityDefinition> _abilityDS;
    [SerializeField] private Data_GAS _gasData;
    public InputActionAsset ActionAsset;
    public string ActionName;
    public bool CancelOnRelease;
    
    private InputAction _abilityAction;
    protected override void OnEnter()
    {
        base.OnEnter();
        _gasData = Owner.GetData<Data_GAS>();
        _abilityDS.GetData(Owner);
        
        _abilityAction = ActionAsset.FindAction(ActionName);
        _abilityAction.performed += OnPerformed;
        _abilityAction.canceled += OnCanceled;
        
        _abilityAction?.Enable();
    }

    protected override void OnExit()
    {
        base.OnExit();
        _abilityAction.performed -= OnPerformed;
        _abilityAction.canceled -= OnCanceled;
    }

    private void OnCanceled(InputAction.CallbackContext obj)
    {
        if(CancelOnRelease)
            _gasData.AbilityController.CancelAbilityIfActive(_abilityDS.Data.AbilityDefinition.name);
    }

    private void OnPerformed(InputAction.CallbackContext obj)
    {
        _gasData.AbilityController.TryActiveAbilityWithDefinition(_abilityDS.Data.AbilityDefinition);
    }
}
