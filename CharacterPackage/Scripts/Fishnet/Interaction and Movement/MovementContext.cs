using FishNet.Object;
using UnityEngine;

public class MovementContext : InteractionContext
{
    private readonly Network_Movement _movementController;
    
    public MovementContext(Network_Movement movement)
    {
        _movementController = movement;
    }
    
    public override int Priority => 0; // Lowest priority
    
    public override ICommand CreateCommand(Vector3 position, GameObject target, NetworkBehaviour source)
    {
        return new MoveCommand(_movementController, position);
    }
    
    public override bool CanHandle(GameObject target)
    {
        // Can handle any ground or empty space
        return target == null || target.layer == LayerMask.NameToLayer("Ground");
    }
}