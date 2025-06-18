using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

public class HitReactionEventTrigger : ActorBehaviour
{
    [SerializeField] private EventField<HitNotifyArgs> _hitEvent;

    [SerializeField] private float _forceMultiplier;

    private HitReaction _hitReaction;
    protected override void OnStart()
    {
        base.OnStart();
        _hitReaction = GetComponent<HitReaction>();
        _hitEvent.Register(Actor,OnHit);
    }

    private void OnDestroy()
    {
        _hitEvent.Unregister(Actor,OnHit);
    }

    private void OnHit(EventArgs arg1, HitNotifyArgs arg2)
    {
        _hitReaction.Hit(arg2.Collider, arg2.Direction.normalized * _forceMultiplier, arg2.Position);
    }
}
