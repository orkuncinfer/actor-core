using System.Collections.Generic;
using UnityEngine;

public class MultiStateComponent : MonoState
{
    private List<MonoState> _runningStates = new List<MonoState>();
    protected override void OnEnter()
    {
        base.OnEnter();
        
        MonoState[] childStates = GetComponents<MonoState>();
        foreach (var state in childStates)
        {
            if(state == this) continue;
            _runningStates.Add(state);
            state.onStateFinished += OnChildStateFinished;
            state.CheckoutEnter(Owner);
        }
    }

    private void OnChildStateFinished(MonoState obj)
    {
        Debug.Log("Child state finished");
        bool allFinished = true;

        foreach (MonoState childState in _runningStates)
        {
            if(!childState.IsFinished) allFinished = false;
            Debug.Log(childState.name + " is finished: " + childState.IsFinished);
        }
        obj.onStateFinished -= OnChildStateFinished;
        
        if(allFinished) CheckoutExit();
    }

    protected override void OnExit()
    {
        base.OnExit();
        foreach (var state in _runningStates)
        {
            if(state == this) continue;
            state.CheckoutExit();
        }
    }
}