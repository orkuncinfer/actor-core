using UnityEngine;

public abstract class StateActions : ScriptableObject
{
    public StateManager Manager;
    public virtual void OnTick(){}
    public virtual void OnEnter(){}
    public virtual void OnExit(){}
}