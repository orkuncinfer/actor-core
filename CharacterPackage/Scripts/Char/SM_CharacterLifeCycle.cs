using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_CharacterLifeCycle : ActorStateMachine
{
    [SerializeField] private MonoState _alive;
    [SerializeField] private MonoState _dead;
    [SerializeField] private MonoState _revive;
    
    [SerializeField] private GameplayTag AbilityStateTag;
 
    [SerializeField] private Data_Living _livingData;
    
    protected override MonoState _initialState => _alive;

    protected override void OnEnter()
    {
        base.OnEnter();
        _livingData = Owner.GetData<Data_Living>();
    }

    protected override void OnRequireAddTransitions()
    {
        AddTransition(_alive,_dead, AliveToDead);
    }

    private bool AliveToDead()
    {
        if(_livingData.ShouldDieTrigger)
        {
            _livingData.ShouldDieTrigger = false;
            return true;
        }

        return false;
    }

}
