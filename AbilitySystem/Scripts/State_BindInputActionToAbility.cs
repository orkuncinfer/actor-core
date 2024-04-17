using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class State_BindInputActionToAbility : MonoState
{
    [SerializeField] private DataGetter<Data_AbilityDefinition> _abilityData;
    public InputActionAsset ActionAsset;
    public string ActionName;
    
    private InputAction _abilityAction;
    protected override void OnEnter()
    {
        base.OnEnter();
        _abilityData.GetData(Owner);
        
        _abilityAction = ActionAsset.FindAction(ActionName);
        _abilityAction.performed += OnPerformed;
        
        _abilityAction?.Enable();
    }
    
    private void OnPerformed(InputAction.CallbackContext obj)
    {
        Owner.GetComponent<AbilityController>().TryActiveAbilityWithDefinition(_abilityData.Data.AbilityDefinition);
    }
}
