using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class ActorStateMachine : MonoState
{
    protected abstract MonoState _initialState { get; }
    [BoxGroup("space",false)][ShowInInspector][ReadOnly][DisplayAsString][GUIColor(0.96f,0.91f,0.024f)]protected MonoState _currentState;
    private Dictionary<MonoState, List<Transition>> _transitions = new Dictionary<MonoState, List<Transition>>();

    private List<Transition> _anyTransitions = new List<Transition>();
    
    private class Transition
    {
        public MonoState ToState;
        public System.Func<bool> Condition;

        public Transition(MonoState toState, System.Func<bool> condition)
        {
            ToState = toState;
            Condition = condition;
        }
    }

    public abstract void OnRequireAddTransitions();

    public override void OnInitialize()
    {
        base.OnInitialize();
    }

    protected override void OnEnter()
    {
        base.OnEnter();
        
        _currentState = _initialState;
        _currentState.CheckoutEnter(Owner);
        OnRequireAddTransitions();
    }

    protected override void OnExit()
    {
        base.OnExit();
        _currentState.CheckoutExit();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (_currentState != null)
        {
            foreach (var transition in _anyTransitions)
            {
                if (transition.Condition())
                {
                    SetState(transition.ToState);
                    return; 
                }
            }

            if (_transitions.TryGetValue(_currentState, out var currentTransitions))
            {
                foreach (var transition in currentTransitions)
                {
                    if (transition.Condition())
                    {
                        SetState(transition.ToState);
                        break;
                    }
                }
            }
        }
    }

    public void AddTransition(MonoState fromState, MonoState toState, System.Func<bool> condition)
    {
        if (!_transitions.ContainsKey(fromState))
        {
            _transitions[fromState] = new List<Transition>();
        }
        _transitions[fromState].Add(new Transition(toState, condition));
    }
    public void AddAnyTransition(MonoState toState, System.Func<bool> condition)
    {
        _anyTransitions.Add(new Transition(toState, condition));
    }

    public void SetState(MonoState newState)
    {
        if (_currentState == newState) return;

        if (_currentState != null)
            _currentState.CheckoutExit();

        _currentState = newState;
        _currentState.CheckoutEnter(Owner);
    }
}