using FishNet.Object;
using UnityEngine;

public class AttackContext : InteractionContext
{
    private readonly Service_GAS _abilitySystem;
    private readonly Network_Movement _movementController;
    private readonly float _attackRange;
    private readonly GameplayTag _attackAbilityTag;
    private readonly ActorBase _targetActor;
    
    public AttackContext(Service_GAS abilitySystem, Network_Movement movement, float attackRange, GameplayTag attackAbilityTag)
    {
        _abilitySystem = abilitySystem;
        _movementController = movement;
        _attackRange = attackRange;
        _attackAbilityTag = attackAbilityTag;
    }
    
    public override int Priority => 10; // High priority
    
    public override ICommand CreateCommand(Vector3 position, GameObject target, NetworkBehaviour source)
    {
        return new AttackCommand(_abilitySystem, _movementController, target, _attackRange,_attackAbilityTag);
    }
    
    public override bool CanHandle(GameObject target)
    {
        // Can handle enemy units or attackable objects
        return target != null && target != _movementController.gameObject &&
               (/*target.CompareTag("Enemy") ||*/ target.GetComponent<IAttackable>() != null);
    }
}