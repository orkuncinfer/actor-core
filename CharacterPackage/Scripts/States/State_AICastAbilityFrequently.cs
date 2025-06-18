using System.Collections;
using Pathfinding;
using UnityEngine;

public class State_AICastAbilityFrequently : MonoState
{
    public AbilityDefinition Ability;
    public float CastDelay;
    public bool CancelAfterAFrame;
    private Transform _movementTransform => Owner.transform;
    private Service_GAS _gas;
    private float _nextCastTime;

    private ActiveAbility _activeAbility;
    protected override void OnEnter()
    {
        base.OnEnter();
        _gas = Owner.GetService<Service_GAS>();
        
    }
    

    protected override void OnExit()
    {
        base.OnExit();

    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (Time.time > _nextCastTime && _activeAbility == null)
        {
            StartCoroutine(StartAbility());
        }
    }

    IEnumerator StartAbility()
    {
        _activeAbility = _gas.AbilityController.AddAndTryActivateAbility(Ability);
        if (_activeAbility != null)
        {
            _activeAbility.onFinished += ability =>
            {
                _activeAbility = null;
                _nextCastTime = Time.time + CastDelay;
            };
            if (CancelAfterAFrame)
            {
                yield return null;
                _gas.AbilityController.CancelAbilityIfActive(_activeAbility);
            }
        }
        else
        {
            
        }
    }
}