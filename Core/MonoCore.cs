using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoCore : MonoBehaviour
{
    private bool _initialized;
    private void Awake()
    {
        GlobalActorEvents.onActorsInitialized += OnActorsInitialized;
    }

    private void OnDestroy()
    {
        GlobalActorEvents.onActorsInitialized -= OnActorsInitialized;
    }

    private void OnActorsInitialized()
    {
        OnReady();
        _initialized = true;
    }

    protected virtual void OnReady()
    {
        
    }

    private void OnEnable()
    {
        if (_initialized)
        {
            OnReady();
        }
    }
}
