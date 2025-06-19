using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ActorStarterOnEvent : MonoBehaviour
{
    [SerializeField] private Actor _actor;
    [SerializeField] private EventField _eventField;
    [SerializeField] private EventField _stopEvent;

    private void Awake()
    {
        _eventField.Register(null, OnEventRaised);
        _stopEvent.Register(null, OnStopEventRaised);
    }

    private void OnStopEventRaised(EventArgs obj)
    {
        _actor.StopIfNot();
    }

    private void OnEventRaised(EventArgs obj)
    {
        _actor.StartIfNot();
    }
}
