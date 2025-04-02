using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class State_GameModeEnded : MonoState
{
    private DS_GameModeRuntime _gameModeRuntimeData;
    protected override void OnEnter()
    {
        base.OnEnter();
        _gameModeRuntimeData = Owner.GetData<DS_GameModeRuntime>();
        GlobalActorEvents.SetGameModeStopped();
        
        List<Actor> actors = ActorRegistry.Actors;
        foreach (Actor actor in actors)
        {
            if(actor == Owner) continue;
            if(actor.StartMethod != ActorStartMethods.PlayMode) continue;
            actor.StopIfNot();
            _gameModeRuntimeData.StartedActors.Remove(actor);
        }
    }
}