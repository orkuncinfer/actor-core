using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class State_GameModeInitialize : MonoState
{
    private DS_GameMode _gameModeData;
    protected override void OnEnter()
    {
        base.OnEnter();
        _gameModeData = Owner.GetData<DS_GameMode>();
        
        Actor[] actors = FindObjectsOfType<Actor>();
        foreach (Actor actor in actors)
        {
            if(actor == Owner) continue;
            actor.StartIfNot();
            _gameModeData.StartedActors.Add(actor);
        }
        
        _gameModeData.ResetAllVariables();
        CheckoutExit();
    }

    [Button]
    void finished()
    {
        Debug.Log(IsFinished);
    }
}
