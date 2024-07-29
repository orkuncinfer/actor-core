using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class State_GameModeInitialize : MonoState
{
    private DS_GameModeRuntime _gameModeRuntimeData;
    [FormerlySerializedAs("_modeData")] [SerializeField] private DSGetter<DS_GameModeRuntime> _modeDS;
    private EventSignal _event;
    protected override void OnEnter()
    {
        base.OnEnter();
        _modeDS.GetData();
        _gameModeRuntimeData = _modeDS.Data;
        
        Actor[] actors = FindObjectsOfType<Actor>();
        foreach (Actor actor in actors)
        {
            if(actor == Owner) continue;
            if(actor.StartMethod != ActorStartMethods.Auto) continue;
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
