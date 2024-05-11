using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorBehaviour : MonoBehaviour
{
    private void Awake()
    {
        if(GlobalActorEvents.ActorsInitialized)
            OnStart();
        else
            GlobalActorEvents.onActorsInitialized += OnStart;
    }

    protected virtual void OnStart() {  }
}
