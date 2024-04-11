using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityInputActions : MonoBehaviour
{
    public InputActionAsset ActionAsset;
    private InputAction AbilityAction;
    public AbilityDefinition AbilityDefinition;
    public string ActionName;

    private void Awake()
    {
        AbilityAction = ActionAsset.FindAction(ActionName);
        AbilityAction.performed += OnPerformed;
        
        AbilityAction?.Enable();
    }
    
    private void OnPerformed(InputAction.CallbackContext obj)
    {
        Debug.Log("performed");
        GetComponent<AbilityController>().TryActiveAbilityWithDefinition(AbilityDefinition);
    }
}
