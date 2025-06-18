using System.Collections;
using UnityEngine;

public class CollectCommand : ICommand
{
    private readonly Network_Movement _movement;
    private readonly PathfindingFollowable _targetFollowable;
    private readonly LootLabel _lootLabel;
    private Coroutine _collectCoroutine;
    
    public bool IsComplete { get; private set; }
    public float Priority => 5;
    
    public CollectCommand(Network_Movement movement, LootLabel lootLabel)
    {
        _movement = movement;
        _lootLabel = lootLabel;
    }
    
    public void Execute()
    {
        _collectCoroutine = _movement.StartCoroutine(CollectItem());
        IsComplete = false;
    }
    
    public void Cancel()
    {
        if (_collectCoroutine != null)
        {
            _movement.StopCoroutine(_collectCoroutine);
            _movement.StopMovement();
        }
        IsComplete = true;
    }
    
    private IEnumerator CollectItem()
    {
        // Move to the item
        yield return _movement.MoveTo(_lootLabel.TargetTransform.position);
        
        _lootLabel.Collect();
        // Notify that target was reached (existing logic)
        //_targetFollowable.TargetReached(_movement.NetworkObject);
        
        IsComplete = true;
    }
}