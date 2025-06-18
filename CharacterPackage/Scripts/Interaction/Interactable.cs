using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private AbilityDefinition _interactAbility;
    [SerializeField] private float _interactionCooldown;
    [SerializeField] private GameObject _interactionUI;
    [SerializeField] private Vector3 _displayOffset;
    [SerializeField] private float _displayScale = 1;
    [SerializeField][ReadOnly]private bool _isInteractable;
    
    public  event Action<ActorBase,string> OnInteractAttempt;
    public bool IsInteractable
    {
        get => _isInteractable;
        set => _isInteractable = value;
    }

    private bool _showing;

    private GameObject _instance;
    
    public void SetInteractable(bool interactable)
    {
        _isInteractable = interactable;


        if (interactable && !_showing)
        {
            _showing = true;
            _instance = PoolManager.SpawnObject(_interactionUI, transform.position + _displayOffset, Quaternion.identity);
            _instance.transform.forward = Camera.main.transform.forward;
            _instance.transform.localScale = Vector3.one * _displayScale;
        }
        
        if (!interactable && _showing)
        {
            _showing = false;
            PoolManager.ReleaseObject(_instance);
        }
    }

    public void InteractAttempt(ActorBase owner,string actionName)
    {
        OnInteractAttempt?.Invoke(owner,actionName);
    }
}
