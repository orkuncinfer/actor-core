using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class State_GameModeInitialize : MonoState
{
    private DS_GameModeRuntime _gameModeRuntimeData;
    private EventSignal _event;
    protected override void OnEnter()
    {
        base.OnEnter();
        _gameModeRuntimeData = Owner.GetData<DS_GameModeRuntime>();
        
        Actor[] actors = FindObjectsOfType<Actor>();
        foreach (Actor actor in actors)
        {
            if(actor == Owner) continue;
            ActorRegistry.RegisterActor(actor);
            
            if(actor.StartMethod != ActorStartMethods.OnInitialize) continue;
            actor.StartIfNot();
            _gameModeRuntimeData.StartedActors.Add(actor);
       
        }
        
        _gameModeRuntimeData.ResetAllVariables();
        
        GlobalActorEvents.SetActorsInitialized();
        CheckoutExit();
    }

    [Button]
    void finished()
    {
        DDebug.Log(IsFinished);
    }
}
