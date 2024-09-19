using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActorService
{
    protected ActorBase Actor { get; set; }
    
    protected object Service { get; set; }
    
    public virtual void RegisterService()
    {
        if(Service is MonoBehaviour mono)
        {
            Actor = ActorUtilities.FindFirstActorInParents(mono.transform);
            Actor.AddService(Service);
        }
    }
}
