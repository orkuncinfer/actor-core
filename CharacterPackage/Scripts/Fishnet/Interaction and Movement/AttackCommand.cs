using System.Collections;
using FishNet;
using FishNet.Object;
using UnityEngine;

public struct AbilityRequestArgs
{
    public GameplayTag AbilityTag;
    public string AbilityId;
    public GameObject Target;
}
public class AttackCommand : ICommand
{
    private readonly Service_GAS _abilitySystem;
    private readonly Network_Movement _movement;
    private Network_AbilityNetworkLayer _networkAbility;
    private readonly GameObject _target;
    private readonly float _attackRange;
    private Coroutine _attackCoroutine;
    private readonly GameplayTag _attackAbilityTag;
    private readonly WaitForSeconds _delay = new WaitForSeconds(0.5f);

    private NetworkObject _targetNetwork;
    private ActorBase _targetActor;
        
    public bool IsComplete { get; private set; }
    public float Priority => 10;
    
    public AttackCommand(Service_GAS abilitySystem, Network_Movement movement, GameObject target, float attackRange, GameplayTag attackAbilityTag)
    {
        _abilitySystem = abilitySystem;
        _movement = movement;
        _target = target;
        _attackRange = attackRange;
        _attackAbilityTag = attackAbilityTag;
        _targetActor = _target.GetComponent<ActorBase>();
    }
    
    public void Execute()
    {
        _networkAbility = _movement.GetComponent<Network_AbilityNetworkLayer>();
        _attackCoroutine = _movement.StartCoroutine(AttackSequence());
        IsComplete = false;
        
        // Set target in ability system immediately
        _targetNetwork = _target.GetComponent<NetworkObject>();
        //_abilitySystem.AbilityController.Target = _target;
    }
    
    public void Cancel()
    {
        if (_attackCoroutine != null)
        {
            _movement.StopCoroutine(_attackCoroutine);
            _movement.StopMovement();
        }
        
        _abilitySystem.AbilityController.Target = null;
        IsComplete = true;
    }
    
    private IEnumerator AttackSequence()
    {
        // Move to attack range
        yield return _movement.MoveToTarget(_target, _attackRange);

        // Check if target is still valid
        if (_target == null || !_target.activeInHierarchy)
        {
            IsComplete = true;
            yield break;
        }
        
        // Ensure we're facing the target
        yield return _movement.FaceTarget(_target);
        
     
        
        // Attempt to activate ability
        while (_target != null && Vector3.Distance(_movement.transform.position, _target.transform.position) <= _attackRange)
        {
            if (_abilitySystem.AbilityController.CanActivateAbility(_attackAbilityTag))
            {
                Debug.Log("Request activate ability");
                int networkId = 0;
                if (_targetNetwork)
                {
                    networkId = _targetNetwork.ObjectId;
                }

                Debug.Log("target actor " + _targetActor.gameObject.name);
                if (_targetActor.GameplayTags.HasTag(GameplayTagManager.RequestTag("Status.Dead")))
                {
                    Debug.Log("want cancel");
                    Cancel();
                    yield break;
                }
                _networkAbility.RequestAbility(_attackAbilityTag,networkId);
                //_abilitySystem.AbilityController.TryActivateAbilityWithGameplayTag(_attackAbilityTag);
                yield return _delay;
                //send to server
                /*if (_abilitySystem.AbilityController.TryActivateAbility("BasicAttack", _target))
                {
                    yield return _delay;
                }*/
            }
            
            else
            {
                yield return _delay;
            }
         
            // If target moved out of range, pursue
            if (_target != null && Vector3.Distance(_movement.transform.position, _target.transform.position) > _attackRange)
            {
                yield return _movement.MoveToTarget(_target, _attackRange);
            }
        }
        
        IsComplete = true;
    }
}