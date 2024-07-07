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
    
    private InputAction _abilityAction;
    protected override void OnEnter()
    {
        base.OnEnter();
        _gasData = Owner.GetData<Data_GAS>();
        _abilityDS.GetData(Owner);
        
        _abilityAction = ActionAsset.FindAction(ActionName);
        _abilityAction.performed += OnPerformed;
        
        _abilityAction?.Enable();
    }
    
    private void OnPerformed(InputAction.CallbackContext obj)
    {
        _gasData.AbilityController.TryActiveAbilityWithDefinition(_abilityDS.Data.AbilityDefinition);
    }
}
