using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class State_EquipHandling : MonoState
{
    [SerializeField] private Data_GAS _gasData;
    public InputActionAsset ActionAsset;

    [SerializeField] private string[] EquipActionNames;
        
    protected override void OnEnter()
    {
        base.OnEnter();
        _gasData = Owner.GetData<Data_GAS>();
            
        foreach (var abilityInfo in EquipActionNames)
        {
            var abilityAction = ActionAsset.FindAction(abilityInfo);
            abilityAction.performed += OnPerformed;
            abilityAction?.Enable();
        }
    }
    
    protected override void OnExit()
    {
        base.OnExit();
        foreach (var abilityInfo in EquipActionNames)
        {
            var abilityAction = ActionAsset.FindAction(abilityInfo);
            abilityAction.performed -= OnPerformed;
        }
    }
    
  
    
    private void OnPerformed(InputAction.CallbackContext obj)
    {
        
        /* _gasData.AbilityController.TryActiveAbilityWithDefinition(abilityTriggerInfo.AbilityDefinition, out ActiveAbility activatedAbility);
            
        if (activatedAbility != null)
        {
            IsBusy = true;
            activatedAbility.onFinished += OnAbilityFinished;
        }*/
    }

    private void OnAbilityFinished(ActiveAbility obj)
    {
        obj.onFinished -= OnAbilityFinished;
    }
}