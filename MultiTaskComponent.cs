
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MultiTaskComponent : MonoState
{
    [SerializeField] private bool _isLooping;
    private MonoState[] _childStates;
    private int _currentStateIndex;

    private List<MonoState> _inOrderStatesList = new List<MonoState>();
    protected override void OnEnter()
    {
        base.OnEnter();
        _inOrderStatesList.Clear();
        _childStates = GetComponents<MonoState>();
        foreach (MonoState state in _childStates)
        {
            if(state==this) continue;
            _inOrderStatesList.Add(state);
        }
        _inOrderStatesList[0].CheckoutEnter(Owner);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (_inOrderStatesList[_currentStateIndex].IsFinished)
        {
            if (_currentStateIndex + 1 == _inOrderStatesList.Count)
            {
                if (_isLooping)
                {
                    _currentStateIndex = 0;
                    foreach (MonoState state in _inOrderStatesList)
                    {
                        state.Reset();
                        _inOrderStatesList[_currentStateIndex].CheckoutEnter(Owner);
                    }
                }
                else
                {
                    CheckoutExit();
                }
            }
            else
            {
                _currentStateIndex++;
                _inOrderStatesList[_currentStateIndex].CheckoutEnter(Owner);
            }
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
        Reset();
    }
}