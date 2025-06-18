using System;
using UnityEngine;
using FishNet.Object;

// Base command interface for all actions
public interface ICommand
{
    void Execute();
    void Cancel();
    bool IsComplete { get; }
    float Priority { get; }
}

// Base class for contextual behaviors
public abstract class InteractionContext
{
    public virtual int Priority => 0;
    public abstract ICommand CreateCommand(Vector3 position, GameObject target, NetworkBehaviour source);
    public abstract bool CanHandle(GameObject target);
}