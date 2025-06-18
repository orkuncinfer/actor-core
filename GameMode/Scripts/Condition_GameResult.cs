using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Condition_GameResult : StateCondition
{
    public bool ShouldCompleted;

    private DS_GameModeRuntime _gameModeRuntime;

    public override void Initialize(ActorBase owner)
    {
        base.Initialize(owner);
        _gameModeRuntime = Owner.GetData<DS_GameModeRuntime>();
    }

    public override bool CheckCondition()
    {
        Debug.Log("Checking for game result"  + _gameModeRuntime.Completed);
        AddWatchingValue( "IsCompleted "+ _gameModeRuntime.Completed);
        
        return _gameModeRuntime.Completed == ShouldCompleted;
        
    }
}