using System.Collections.Generic;
using Sirenix.OdinInspector;

public class State_GameModePlaying : MonoState
{
    private DS_GameModeRuntime _gameModeRuntimeData;
    private EventSignal _event;
    protected override void OnEnter()
    {
        base.OnEnter();
        _gameModeRuntimeData = Owner.GetData<DS_GameModeRuntime>();
        

        List<Actor> actors = ActorRegistry.Actors;
        foreach (Actor actor in actors)
        {
            if(actor == Owner) continue;
            if(actor.StartMethod != ActorStartMethods.PlayMode) continue;
            actor.StartIfNot();
            _gameModeRuntimeData.StartedActors.Add(actor);
        }
        GlobalActorEvents.SetGameModeStarted();
    }

    protected override void OnExit()
    {
        base.OnExit();
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