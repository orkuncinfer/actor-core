using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[Serializable]
public class AbilityTriggerInfo
{
    public AbilityDefinition AbilityDefinition;
    public string ActionName;
    public bool CancelOnRelease;
}

public class State_AbilityTriggerSlot : MonoState
{
        public InputActionAsset ActionAsset;
      
        public List<AbilityTriggerInfo> AbilityTriggerInfos;

        public bool IsBusy;
        
        private Service_GAS _gas;
        
        protected override void OnEnter()
        {
            base.OnEnter();
            _gas = Owner.GetService<Service_GAS>();
            
            foreach (var abilityInfo in AbilityTriggerInfos)
            {
                 var abilityAction = ActionAsset.FindAction(abilityInfo.ActionName);
                 abilityAction.performed += OnPerformed;
                 abilityAction.canceled += OnCanceled;
                 abilityAction?.Enable();
            }
        }
    
        protected override void OnExit()
        {
            base.OnExit();
            foreach (var abilityInfo in AbilityTriggerInfos)
            {
                var abilityAction = ActionAsset.FindAction(abilityInfo.ActionName);
                abilityAction.performed -= OnPerformed;
                abilityAction.canceled -= OnCanceled;
            }
        }
    
        private void OnCanceled(InputAction.CallbackContext obj)
        {
            AbilityTriggerInfo abilityTriggerInfo = AbilityTriggerInfos.Find(info => info.ActionName == obj.action.name);
            if(abilityTriggerInfo.CancelOnRelease)
                _gas.AbilityController.CancelAbilityIfActive(abilityTriggerInfo.AbilityDefinition.name);
        }
    
        private void OnPerformed(InputAction.CallbackContext obj)
        {
            AbilityTriggerInfo abilityTriggerInfo = AbilityTriggerInfos.Find(info => info.ActionName == obj.action.name);
            ActiveAbility activatedAbility = _gas.AbilityController.TryActiveAbilityWithDefinition(abilityTriggerInfo.AbilityDefinition);
            
            if (activatedAbility != null)
            {
                IsBusy = true;
                activatedAbility.onFinished += OnAbilityFinished;
            }
        }

        private void OnAbilityFinished(ActiveAbility obj)
        {
            obj.onFinished -= OnAbilityFinished;
            IsBusy = false;
        }
}
