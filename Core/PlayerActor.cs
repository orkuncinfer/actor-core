using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActor : Actor
{
    protected override void Awake()
    {
        base.Awake();
        ActorRegistry.PlayerActor = this;
    }
}
