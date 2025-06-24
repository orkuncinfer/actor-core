using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Interactable))]
public class Interactable_Action : MonoBehaviour
{
    [SerializeField] private string _actionName;
    [SerializeField] private AbilityDefinition _abilityOnAction;
    [SerializeField] private UnityEvent<Actor> _actionOnAbilitySuccess;

    private Interactable _interactable;
    private void Start()
    {
        _interactable = GetComponent<Interactable>();
        _interactable.OnInteractAttempt += OnInteractAttempt;
    }

    private void OnDestroy()
    {
        _interactable.OnInteractAttempt -= OnInteractAttempt;
    }

    private void OnInteractAttempt(Actor owner,string actionName)
    {
        if (actionName.Equals(_actionName, StringComparison.OrdinalIgnoreCase))
        {
            if (owner.GetService<Service_GAS>().AbilityController.AddAndTryActivateAbility(_abilityOnAction) != null)
            {
                _actionOnAbilitySuccess?.Invoke(owner);
            }
        }
    }
}
