using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_BlockInputOnEnter : MonoState
{
    [SerializeField] private bool _blockInputOnEnter;
    [SerializeField] private bool _blockInputOnExit;
    
    private Data_Character _dataCharacter;
    protected override void OnEnter()
    {
        base.OnEnter();
        _dataCharacter = Owner.GetData<Data_Character>();
        
        _dataCharacter.MovementInput.BlockInput = _blockInputOnEnter;
    }

    protected override void OnExit()
    {
        base.OnExit();
        _dataCharacter.MovementInput.BlockInput = _blockInputOnExit;
    }
}
