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
        if(childStates.Length == 1) CheckoutExit();
    }

    private void OnChildStateFinished(MonoState obj)
    {
        bool allFinished = true;

        foreach (MonoState childState in _runningStates)
        {
            if(!childState.IsFinished) allFinished = false;
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