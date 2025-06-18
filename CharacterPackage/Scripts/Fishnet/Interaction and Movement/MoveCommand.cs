using System.Collections;
using UnityEngine;
using FishNet.Object;

// Move Command - handles simple movement to a position
public class MoveCommand : ICommand
{
    private readonly Network_Movement _movement;
    private readonly Vector3 _destination;
    private Coroutine _moveCoroutine;
    
    public bool IsComplete { get; private set; }
    public float Priority => 0;
    
    public MoveCommand(Network_Movement movement, Vector3 destination)
    {
        _movement = movement;
        _destination = destination;
    }
    
    public void Execute()
    {
        _moveCoroutine = _movement.StartCoroutine(MoveToPosition());
        IsComplete = false;
    }
    
    public void Cancel()
    {
        if (_moveCoroutine != null)
        {
            _movement.StopCoroutine(_moveCoroutine);
            _movement.StopMovement();
        }
        IsComplete = true;
    }
    
    private IEnumerator MoveToPosition()
    {
        yield return _movement.MoveTo(_destination);
        IsComplete = true;
    }
}

// Attack Command - moves to target and attacks when in range

// Collect Command - for items and pickups