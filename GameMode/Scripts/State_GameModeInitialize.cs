using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class State_GameModeInitialize : MonoState
{
    private DS_GameMode _gameModeData;
    [SerializeField] private DataGetter<DS_GameMode> _modeData;
    private EventSignal _event;
    protected override void OnEnter()
    {
        base.OnEnter();
        _modeData.GetData();
        _gameModeData = _modeData.Data;
        
        Actor[] actors = FindObjectsOfType<Actor>();
        foreach (Actor actor in actors)
        {
            if(actor == Owner) continue;
            actor.StartIfNot();
            _gameModeData.StartedActors.Add(actor);
        }
        
        _gameModeData.ResetAllVariables();
        
        GlobalActorEvents.SetActorsInitialized();
        CheckoutExit();
    }

    [Button]
    void finished()
    {
        DDebug.Log(IsFinished);
    }
}
