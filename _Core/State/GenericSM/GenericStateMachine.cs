using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class GenericStateMachine : MonoState
{
    #region Name

    [HideLabel][HideIf("_changeNameToggle")]
    [DisplayAsString(false, 20, TextAlignment.Center, true)]
    [InlineButton("A", SdfIconType.Pencil, "")]
    public string Description ;

    [ShowIf("_changeNameToggle")]  [InlineButton("A", SdfIconType.Check, "")][HorizontalGroup("ChangeName")]
    public string ChangeName;
    
    private bool _changeNameToggle;
    
    private void A()
    {
        _changeNameToggle = !_changeNameToggle;
        Description = ChangeName;
    }

    #endregion
    
    public MonoState InitialState;
    [BoxGroup("space",false)][ShowInInspector][ReadOnly][DisplayAsString][GUIColor(0.96f,0.91f,0.024f)]protected MonoState _currentState;
    public MonoState CurrentState => _currentState;
    private Dictionary<MonoState, List<StateTransition>> _transitions = new Dictionary<MonoState, List<StateTransition>>();
    
    [SerializeField] private List<StateTransition> _anyTransitions = new List<StateTransition>();

    public List<StateField> States => _states;
    [SerializeField] private List<StateField> _states;
    
    [Button]
    public void TestAdd()
    {
        StateField stateField = new StateField();
        StateTransition stateTransition = new StateTransition();
    //    Condition_IsAlive conditionIsAlive = System.Activator.CreateInstance(typeof(Condition_IsAlive)) as Condition_IsAlive;
      //  stateTransition.Conditions.Add(conditionIsAlive);
        stateField.Transitions.Add(stateTransition);
        _states.Add(stateField);
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        InitializeStates();
    }

    private void InitializeStates()
    {
        foreach (var stateField in _states)
        {
            _transitions.Add(stateField.State, stateField.Transitions);
            stateField.Transitions.ForEach(x => x.Initialize(Owner));
        }
        _anyTransitions.ForEach(x => x.Initialize(Owner));
    }

    protected override void OnEnter()
    {
        base.OnEnter();
        _currentState = InitialState;
        _currentState.CheckoutEnter(Owner);
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
                bool conditionMet = true;
                foreach (var condition in transition.Conditions)
                {
                    if (!condition.CheckCondition())
                    {
                        conditionMet = false;
                        break;
                    }
                }
                if (conditionMet)
                {
                    SetState(transition.ToState);
                    return;
                }
            }

            if (_transitions.TryGetValue(_currentState, out var currentTransitions))
            {
                foreach (var transition in currentTransitions)
                {
                    bool conditionMet = true;
                    foreach (var condition in transition.Conditions)
                    {
                        if (!condition.CheckCondition())
                        {
                            conditionMet = false;
                            break;
                        }
                    }
                    if (conditionMet)
                    {
                        SetState(transition.ToState);
                        break;
                    }
                }
            }
        }
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