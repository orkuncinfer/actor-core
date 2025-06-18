using FishNet.Object;
using UnityEngine;

public class InteractionContext_Collectible : InteractionContext
{
    private readonly Network_Movement _movementController;
    
    public InteractionContext_Collectible(Network_Movement movement)
    {
        _movementController = movement;
    }
    
    public override int Priority => 5; // Medium priority
    
    public override ICommand CreateCommand(Vector3 position, GameObject target, NetworkBehaviour source)
    {
        var followable = target.GetComponent<LootLabel>();
        return new CollectCommand(_movementController, followable);
    }
    
    public override bool CanHandle(GameObject target)
    {
        return target != null && target.GetComponent<PathfindingFollowable>() != null;
    }
}