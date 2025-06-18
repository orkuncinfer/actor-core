using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class State_StartInteraction : MonoState
{
    [SerializeField] private DSGetter<Data_AbilityDefinition> _abilityDS;

    public InputActionAsset ActionAsset;
    [SerializeField] private AbilityDefinition _interactionAbility;
    public string InteractActionName;
    public string EquipActionName;

    [SerializeField] private PanelActor _interactionInputPanel; 
        
    private InputAction _interactAction;
    private InputAction _equipAction;

    private InteractionSystem _interactionSystem;

    private InteractionTrigger _lastInteractionTrigger;

    private UI_InteractableInputControl _uiInteractableInput;

    protected override void OnEnter()
    {
        base.OnEnter();

        GameObject instance = PoolManager.SpawnObject(_interactionInputPanel.gameObject);
        instance.GetComponent<PanelActor>().StartIfNot();
        _uiInteractableInput = instance.GetComponent<UI_InteractableInputControl>();
        
        _interactionSystem = Owner.GetComponentInChildren<InteractionSystem>();
        _abilityDS.GetData(Owner);

        _interactAction = ActionAsset.FindAction(InteractActionName);
        _equipAction = ActionAsset.FindAction(EquipActionName);

        _equipAction.performed += OnEquipPerformed;
        _interactAction.performed += OnPerformed;

        _interactAction?.Enable();
        _equipAction?.Enable();
    }


    protected override void OnExit()
    {
        base.OnExit();
        _equipAction.performed -= OnEquipPerformed;
        _interactAction.performed -= OnPerformed;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        int closestTriggerIndex = _interactionSystem.GetClosestTriggerIndex();

        // ...if none found, do nothing
        if (closestTriggerIndex == -1)
        {
            RemoveLastInteractable();
            return;
        }

        // ...if the effectors associated with the trigger are in interaction, do nothing
        if (!_interactionSystem.TriggerEffectorsReady(closestTriggerIndex))
        {
            RemoveLastInteractable();
            return;
        }

        if (_lastInteractionTrigger != _interactionSystem.triggersInRange[closestTriggerIndex])
        {
            RemoveLastInteractable();
            _lastInteractionTrigger = _interactionSystem.triggersInRange[closestTriggerIndex];
            if (_lastInteractionTrigger.TryGetComponent<Interactable>(out var interactable))
            {
                _uiInteractableInput.Interactable = interactable;
                interactable.SetInteractable(true);
            }
        }
    }

    private void OnEquipPerformed(InputAction.CallbackContext obj)
    {
        Debug.Log("EquipGroundPerformed");
        if (_lastInteractionTrigger == null) return;
        if (_lastInteractionTrigger.TryGetComponent<Interactable>(out var interactable))
        {
            interactable.InteractAttempt(Owner, "Equip");
        }
    }

    private void OnPerformed(InputAction.CallbackContext obj)
    {
        Debug.Log("InteractPerformed");
        if (_lastInteractionTrigger == null) return;
        if (_lastInteractionTrigger.TryGetComponent<Interactable>(out var interactable))
        {
            interactable.InteractAttempt(Owner, "Interact");
        }
    }

    private void RemoveLastInteractable()
    {
        if (_lastInteractionTrigger != null)
        {
            if (_lastInteractionTrigger.TryGetComponent<Interactable>(out var interactionDisplayer))
            {
                interactionDisplayer.SetInteractable(false);
                _uiInteractableInput.Interactable = null;
            }

            _lastInteractionTrigger = null;
        }
    }
}