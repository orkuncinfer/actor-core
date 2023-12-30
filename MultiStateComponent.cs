using UnityEngine;

public class MultiStateComponent : MonoState
{
    private MonoState[] _childStates;
    protected override void OnEnter()
    {
        base.OnEnter();
        _childStates = GetComponents<MonoState>();
        foreach (var state in _childStates)
        {
            if(state == this) continue;
            state.CheckoutEnter(Owner);
        }
    }

    protected override void OnExit()
    {
        base.OnExit();
        foreach (var state in _childStates)
        {
            if(state == this) continue;
            state.CheckoutExit();
        }
    }
}