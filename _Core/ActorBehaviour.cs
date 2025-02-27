using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorBehaviour : MonoBehaviour
{

    protected ActorBase Actor;
    
    private void Awake()
    {
        Actor = FindFirstActorInParents(transform);
        
        if(GlobalActorEvents.ActorsInitialized)
            OnStart();
        else
            GlobalActorEvents.onActorsInitialized += OnStart;
    }

    protected virtual void OnStart() {  }
    
    public static Actor FindFirstActorInParents(Transform currentParent)
    {
        if (currentParent == null)
        {
            return null;
        }

        Actor actor = currentParent.GetComponent<Actor>();

        if (actor != null)
        {
            return actor;
        }

        return FindFirstActorInParents(currentParent.parent);
    }
}


